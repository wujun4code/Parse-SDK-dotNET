﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using LeanCloud;
using System.Reflection;
using LeanCloud.Realtime.Internal;
using LeanCloud.Storage.Internal;
using System.Threading;

namespace LeanCloud.Realtime
{
    /// <summary>
    /// 实时消息的框架类
    /// 包含了 WebSocket 连接以及事件通知的管理
    /// </summary>
    public class AVRealtime
    {
        private static readonly object mutex = new object();
        private string _wss;
        private string _sesstionToken;
        private long _sesstionTokenExpire;
        private string _clientId;
        private string _tag;

        internal static IAVIMCommandRunner AVIMCommandRunner
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

        internal static IFreeStyleMessageClassingController FreeStyleMessageClassingController
        {
            get
            {
                return AVIMCorePlugins.Instance.FreeStyleClassingController;
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
            Offline = 2,

            /// <summary>
            /// 正在重连
            /// </summary>
            Reconnecting = 3
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

        private EventHandler<AVIMDisconnectEventArgs> m_OnDisconnected;
        /// <summary>
        /// 连接断开触发的事件
        /// <remarks>如果其他客户端使用了相同的 Tag 登录，就会导致当前用户被服务端断开</remarks>
        /// </summary>
        public event EventHandler<AVIMDisconnectEventArgs> OnDisconnected
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

        //public void On(string eventName, Action<IDictionary<string, object>> data)
        //{

        //}

        private void WebSocketClient_OnMessage(string obj)
        {
            AVRealtime.PrintLog("websocket<=" + obj);
            var estimatedData = Json.Parse(obj) as IDictionary<string, object>;
            var notice = new AVIMNotice(estimatedData);
            m_NoticeReceived?.Invoke(this, notice);
        }

        /// <summary>
        /// 设定监听者
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="runtimeHook"></param>
        public void SubscribeNoticeReceived(IAVIMListener listener, Func<AVIMNotice, bool> runtimeHook = null)
        {
            this.NoticeReceived += new EventHandler<AVIMNotice>((sender, notice) =>
            {
                var approved = runtimeHook == null ? listener.ProtocolHook(notice) : runtimeHook(notice) && listener.ProtocolHook(notice);
                if (approved)
                {
                    listener.OnNoticeReceived(notice);
                }
            });
        }

        /// <summary>
        /// 初始化配置项
        /// </summary>
        public struct Configuration
        {
            /// <summary>
            /// 签名工厂
            /// </summary>
            public ISignatureFactory SignatureFactory { get; set; }
            /// <summary>
            /// 自定义 WebSocket 客户端
            /// </summary>
            public IWebSocketClient WebSocketClient { get; set; }
            /// <summary>
            /// LeanCloud App Id
            /// </summary>
            public string ApplicationId { get; set; }
            /// <summary>
            /// LeanCloud App Key
            /// </summary>
            public string ApplicationKey { get; set; }
        }

        /// <summary>
        /// 当前配置
        /// </summary>
        public Configuration CurrentConfiguration { get; internal set; }
        /// <summary>
        /// 初始化实时消息客户端
        /// </summary>
        /// <param name="config"></param>
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

                //this.RegisterMessageType<AVIMMessage>();
                //this.RegisterMessageType<AVIMTextMessage>();
                AVIMMessage.RegisterSubclass<AVIMMessage>();
                AVIMMessage.RegisterSubclass<AVIMTypedMessage>();
                AVIMMessage.RegisterSubclass<AVIMTextMessage>();
            }
        }

        /// <summary>
        /// 初始化实时消息客户端
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="applicationKey"></param>
        public AVRealtime(string applicationId, string applicationKey)
            : this(new Configuration()
            {
                ApplicationId = applicationId,
                ApplicationKey = applicationKey,
            })
        {

        }
        #region websocket log
        internal static Action<string> LogTracker { get; private set; }
        /// <summary>
        /// 打开 WebSocket 日志
        /// </summary>
        /// <param name="trace"></param>
        public static void WebSocketLog(Action<string> trace)
        {
            LogTracker = trace;
        }
        public static void PrintLog(string log)
        {
            if (AVRealtime.LogTracker != null)
            {
                AVRealtime.LogTracker(log);
            }
        }
        #endregion

        /// <summary>
        /// 创建 Client
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="signatureFactory"></param>
        /// <param name="tag"></param>
        /// <param name="deviceId">设备唯一的 Id。如果是 iOS 设备，需要将 iOS 推送使用的 DeviceToken 作为 deviceId 传入</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<AVIMClient> CreateClient(
            string clientId,
            string tag = null,
            string deviceId = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {

            _clientId = clientId;
            _tag = tag;
            if (_tag != null)
            {
                if (deviceId == null)
                    throw new ArgumentNullException(deviceId, "当 tag 不为空时，必须传入当前设备不变的唯一 id(deviceId)");
            }

            if (string.IsNullOrEmpty(clientId)) throw new Exception("当前 ClientId 为空，无法登录服务器。");
            return RouterController.GetAsync(cancellationToken).OnSuccess(_ =>
            {
                _wss = _.Result.server;
                state = Status.Connecting;
                return OpenAsync(_.Result.server);
            }).Unwrap().OnSuccess(t =>
            {
                var cmd = new SessionCommand()
                .UA(VersionString)
                .Tag(tag)
                .Argument("deviceId", deviceId)
                .Option("open")
                .PeerId(clientId);

                return AttachSignature(cmd, this.SignatureFactory.CreateConnectSignature(clientId)).OnSuccess(_ =>
                {
                    return AVIMCommandRunner.RunCommandAsync(cmd);
                }).Unwrap();

            }).Unwrap().OnSuccess(s =>
            {
                if (s.Exception != null)
                {
                    var imException = s.Exception.InnerException as AVIMException;
                }
                state = Status.Online;
                var response = s.Result.Item2;
                if (response.ContainsKey("st"))
                {
                    _sesstionToken = response["st"] as string;
                }
                if (response.ContainsKey("stTtl"))
                {
                    var stTtl = long.Parse(response["stTtl"].ToString());
                    _sesstionTokenExpire = DateTime.Now.UnixTimeStampSeconds() + stTtl;
                }
                PCLWebsocketClient.OnClosed += WebsocketClient_OnClosed;
                PCLWebsocketClient.OnError += WebsocketClient_OnError;
                PCLWebsocketClient.OnMessage += WebSocketClient_OnMessage;
                var client = new AVIMClient(clientId, tag, this);
                return client;
            });
        }

        private void WebsocketClient_OnClosed(int arg1, string arg2, string arg3)
        {
            PrintLog(string.Format("websocket closed with code is {0},reason is {1} and detail is {2}", arg1, arg2, arg3));
            var args = new AVIMDisconnectEventArgs(arg1, arg2, arg3);
            m_OnDisconnected?.Invoke(this, args);
        }

        /// <summary>
        /// 自动重连
        /// </summary>
        /// <returns></returns>
        public Task AutoReconnect()
        {
            return OpenAsync(_wss).ContinueWith(t =>
             {
                 state = Status.Reconnecting;
                 var cmd = new SessionCommand()
                 .UA(VersionString)
                 .Tag(_tag)
                 .R(1)
                 .SessionToken(this._sesstionToken)
                 .Option("open")
                 .PeerId(_clientId);

                 return AttachSignature(cmd, this.SignatureFactory.CreateConnectSignature(_clientId)).OnSuccess(_ =>
                 {
                     return AVIMCommandRunner.RunCommandAsync(cmd);
                 }).Unwrap();
             }).Unwrap().OnSuccess(s =>
             {
                 var result = s.Result;
                 if (result.Item1 == 0)
                 {
                     state = Status.Online;
                 }
                 else
                 {
                     state = Status.Offline;
                 }
             });
        }

        #region register IAVIMMessage
        public void RegisterMessageType<T>() where T: IAVIMMessage
        {
            AVIMCorePlugins.Instance.FreeStyleClassingController.RegisterSubclass(typeof(T));
        }
        #endregion


        /// <summary>
        /// 打开 WebSocket 链接
        /// </summary>
        /// <param name="wss"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal Task OpenAsync(string wss, CancellationToken cancellationToken = default(CancellationToken))
        {
            AVRealtime.PrintLog(wss + " connecting...");
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
                AVRealtime.PrintLog(wss + " connected.");
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
            PrintLog("error:" + obj);
        }

        #region log out and clean event subscribtion
        internal void LogOut()
        {
            this.State = Status.Offline;
            PCLWebsocketClient.OnClosed -= WebsocketClient_OnClosed;
            PCLWebsocketClient.OnError -= WebsocketClient_OnError;
            PCLWebsocketClient.OnMessage -= WebSocketClient_OnMessage;
            m_NoticeReceived = null;
            m_OnDisconnected = null;
            PCLWebsocketClient.Close();
        }
        #endregion

        static AVRealtime()
        {

#if MONO || UNITY
            versionString = "net-unity/" + Version;
#else
            versionString = "net-portable/" + Version;
#endif
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
