using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeanCloud;
using System.Reflection;
using LeanCloud.Realtime.Internal;
using LeanCloud.Storage.Internal;
using System.Threading;

namespace LeanCloud.Realtime
{
    public class AVRealtime
    {
        private static readonly object mutex = new object();
        private string _wss;
        private string _sesstionToken;
        private long _sesstionTokenExpire;
        private string _clientId;
        private string _tag;

        internal static IAVIMCommandRunner AVCommandRunner
        {
            get
            {
                return AVIMCorePlugins.Instance.IMCommandRunner;
            }
        }

        internal IWebSocketClient PCLWebsocketClient
        {
            get
            {
                return AVIMCorePlugins.Instance.WebSocketController;
            }
        }

        internal static IAVRouterController RouterController
        {
            get
            {
                return AVIMCorePlugins.Instance.RouterController;
            }
        }

        /// <summary>
        /// 与云端通讯的状态
        /// </summary>
        public enum Status : int
        {
            /// <summary>
            /// 未初始化
            /// </summary>
            None = -1,

            /// <summary>
            /// 正在连接
            /// </summary>
            Connecting = 0,

            /// <summary>
            /// 已连接
            /// </summary>
            Online = 1,

            /// <summary>
            /// 连接已断开
            /// </summary>
            Offline = 2
        }

        private AVRealtime.Status state;
        public AVRealtime.Status State
        {
            get
            {
                return state;
            }
            private set
            {
                state = value;
            }
        }
        private ISignatureFactory _signatureFactory;

        /// <summary>
        /// 签名接口
        /// </summary>
        public ISignatureFactory SignatureFactory
        {
            get
            {
                if (_signatureFactory == null)
                {
                    if (useLeanEngineSignaturFactory)
                    {
                        _signatureFactory = new LeanEngineSignatureFactory();
                    }
                    else
                    {
                        _signatureFactory = new DefaulSiganatureFactory();
                    }
                }
                return _signatureFactory;
            }
            set
            {
                _signatureFactory = value;
            }
        }

        private bool useLeanEngineSignaturFactory;
        /// <summary>
        /// 启用 LeanEngine 云函数签名
        /// </summary>
        public void UseLeanEngineSignatureFactory()
        {
            useLeanEngineSignaturFactory = true;
        }

        private EventHandler<AVIMEventArgs> m_OnDisconnected;
        public event EventHandler<AVIMEventArgs> OnDisconnected
        {
            add
            {
                m_OnDisconnected += value;
            }
            remove
            {
                m_OnDisconnected -= value;
            }
        }

        private EventHandler<AVIMNotice> m_NoticeReceived;
        public event EventHandler<AVIMNotice> NoticeReceived
        {
            add
            {
                m_NoticeReceived += value;
            }
            remove
            {
                m_NoticeReceived -= value;
            }
        }

        private void WebSocketClient_OnMessage(string obj)
        {
            var estimatedData = Json.Parse(obj) as IDictionary<string, object>;
            var notice = new AVIMNotice(estimatedData);
            m_NoticeReceived?.Invoke(this, notice);
        }

        public void SubscribeNoticeReceived(Action<AVIMNotice> subscriber)
        {
            this.NoticeReceived += new EventHandler<AVIMNotice>((sender, notice) =>
            {
                subscriber(notice);
            });
        }

        /// <summary>
        /// 初始化配置项
        /// </summary>
        public struct Configuration
        {
            public ISignatureFactory SignatureFactory { get; set; }
            public IWebSocketClient WebSocketClient { get; set; }
            public string ApplicationId { get; set; }
            public string ApplicationKey { get; set; }
        }

        public Configuration CurrentConfiguration { get; internal set; }
        public AVRealtime(Configuration config)
        {
            lock (mutex)
            {
                AVClient.Initialize(config.ApplicationId, config.ApplicationKey);
                CurrentConfiguration = config;
                if (CurrentConfiguration.WebSocketClient != null)
                {
                    AVIMCorePlugins.Instance.WebSocketController = CurrentConfiguration.WebSocketClient;
                }

            }
        }

        public AVRealtime(string applicationId, string applicationKey)
            : this(new Configuration()
            {
                ApplicationId = applicationId,
                ApplicationKey = applicationKey,
            })
        {

        }

        /// <summary>
        /// 创建 Client
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="signatureFactory"></param>
        /// <param name="tag"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<AVIMClient> CreateClient(string clientId, ISignatureFactory signatureFactory = null, string tag = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            _clientId = clientId;
            _tag = tag;
            if (signatureFactory != null)
            {
                CurrentConfiguration = new Configuration()
                {
                    ApplicationId = CurrentConfiguration.ApplicationId,
                    ApplicationKey = CurrentConfiguration.ApplicationKey,
                    SignatureFactory = signatureFactory,
                    WebSocketClient = CurrentConfiguration.WebSocketClient
                };
            }

            if (string.IsNullOrEmpty(clientId)) throw new Exception("当前 ClientId 为空，无法登录服务器。");
            state = Status.Connecting;
            return RouterController.GetAsync(cancellationToken).OnSuccess(_ =>
            {
                _wss = _.Result.server;
                return OpenAsync(_.Result.server);
            }).Unwrap().OnSuccess(t =>
            {
                var cmd = new SessionCommand()
                .UA(VersionString)
                .Tag(tag)
                .Option("open")
                .AppId(AVClient.CurrentConfiguration.ApplicationId)
                .PeerId(clientId);

                return AttachSignature(cmd, this.SignatureFactory.CreateConnectSignature(clientId)).OnSuccess(_ =>
                {
                    return AVCommandRunner.RunCommandAsync(cmd);
                }).Unwrap();

            }).Unwrap().OnSuccess(s =>
            {
                state = Status.Online;
                var response = s.Result.Item2;
                _sesstionToken = response["st"].ToString();
                var stTtl = long.Parse(response["stTtl"].ToString());
                _sesstionTokenExpire = DateTime.Now.UnixTimeStampSeconds() + stTtl;
                PCLWebsocketClient.OnClosed += WebsocketClient_OnClosed;
                PCLWebsocketClient.OnError += WebsocketClient_OnError;
                PCLWebsocketClient.OnMessage += WebSocketClient_OnMessage;
                var client = new AVIMClient(clientId, tag, this);

                return client;
            });
        }

        internal Task AutoReconnect()
        {
            return OpenAsync(_wss).ContinueWith(t =>
             {
                 var cmd = new SessionCommand()
                 .UA(VersionString)
                 .Tag(_tag)
                 .R(1)
                 .SessionToken(this._sesstionToken)
                 .Option("open")
                 .AppId(AVClient.CurrentConfiguration.ApplicationId)
                 .PeerId(_clientId);

                 return AttachSignature(cmd, this.SignatureFactory.CreateConnectSignature(_clientId)).OnSuccess(_ =>
                 {
                     return AVCommandRunner.RunCommandAsync(cmd);
                 }).Unwrap();
             }).Unwrap();
        }


        /// <summary>
        /// 打开 WebSocket 链接
        /// </summary>
        /// <param name="wss"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal Task OpenAsync(string wss, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<bool>();
            Action<string> onError = null;
            onError = ((reason) =>
            {
                PCLWebsocketClient.OnError -= onError;
                tcs.SetResult(false);
                tcs.TrySetException(new AVIMException(AVIMException.ErrorCode.FromServer, "try to open websocket at " + wss + "failed.The reason is " + reason, null));
            });

            Action onOpend = null;
            onOpend = (() =>
            {
                PCLWebsocketClient.OnError -= onError;
                PCLWebsocketClient.OnOpened -= onOpend;
                tcs.SetResult(true);
            });

            PCLWebsocketClient.OnOpened += onOpend;
            PCLWebsocketClient.OnError += onError;
            PCLWebsocketClient.Open(wss);
            return tcs.Task;
        }

        internal Task<AVIMCommand> AttachSignature(AVIMCommand command, Task<AVIMSignature> SignatureTask)
        {
            var tcs = new TaskCompletionSource<AVIMCommand>();
            if (SignatureTask == null)
            {
                tcs.SetResult(command);
                return tcs.Task;
            }
            return SignatureTask.OnSuccess(_ =>
            {
                if (_.Result != null)
                {
                    var signature = _.Result;
                    command.Argument("t", signature.Timestamp);
                    command.Argument("n", signature.Nonce);
                    command.Argument("s", signature.SignatureContent);
                }
                return command;
            });
        }

        private void WebsocketClient_OnError(string obj)
        {
            var eventArgs = new AVIMEventArgs() { Message = obj };
            m_OnDisconnected?.Invoke(this, eventArgs);
        }

        private void WebsocketClient_OnClosed()
        {
            AutoReconnect();
        }

        static AVRealtime()
        {
            versionString = "net-portable/" + Version;
        }

        private static readonly string versionString;
        internal static string VersionString
        {
            get
            {
                return versionString;
            }
        }

        internal static System.Version Version
        {
            get
            {
                AssemblyName assemblyName = new AssemblyName(typeof(AVRealtime).GetTypeInfo().Assembly.FullName);
                return assemblyName.Version;
            }
        }
    }
}
