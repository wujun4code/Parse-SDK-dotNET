using LeanCloud;
using LeanCloud.Internal;
using LeanMessage.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeanMessage
{

    /// <summary>
    /// 代表一个实时通信的终端用户
    /// </summary>
    public class AVIMClient
    {
        /// <summary>
        /// 客户端的标识,在一个 Application 内唯一。
        /// </summary>
        public readonly string clientId;


        private static readonly IAVIMPlatformHooks platformHooks;

        //internal static IAVIMPlatformHooks PlatformHooks { get { return platformHooks; } }

        private IWebSocketClient websocketClient
        {
            get
            {
                return platformHooks.WebSocketClient;
            }
        }

        private static readonly IAVIMCommandRunner commandRunner;

        internal static IAVIMCommandRunner AVCommandRunner { get { return commandRunner; } }

        internal static IAVRouterController RouterController
        {
            get
            {
                return AVIMCorePlugins.Instance.RouterController;
            }
        }

        /// <summary>
        /// 创建 AVIMClient 对象
        /// </summary>
        /// <param name="clientId"></param>
        public AVIMClient(string clientId)
        {
            this.clientId = clientId;
        }

        /// <summary>
        /// 创建与 LeanMessage 云端的长连接
        /// </summary>
        /// <returns></returns>
        public Task ConnectAsync()
        {
            return ConnectAsync(CancellationToken.None);
        }

        /// <summary>
        /// 创建与 LeanMessage 云端的长连接
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task ConnectAsync(CancellationToken cancellationToken)
        {
            return RouterController.GetAsync(cancellationToken).OnSuccess(_ =>
            {
                return OpenAsync(_.Result.server);
            }).Unwrap().OnSuccess(t =>
            {
                var cmd = new SessionCommand()
                .UA(platformHooks.ua + Version)
                .Option("open")
                .AppId(AVClient.ApplicationId)
                .PeerId(clientId);

                return AVIMClient.AVCommandRunner.RunCommandAsync(cmd);
            }).Unwrap().OnSuccess(s => 
            {
                var response = s.Result.Item2;
            });
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
            websocketClient.Open(wss);
            Action onOpend = null;
            onOpend = (() =>
            {
                websocketClient.OnOpened -= onOpend;
                tcs.SetResult(true);
            });
            websocketClient.OnOpened += onOpend;

            Action<string> onError = null;
            onError = ((reason) =>
            {
                websocketClient.OnError -= onError;
                tcs.SetResult(false);
                tcs.TrySetException(new AVIMException(AVIMException.ErrorCode.FromServer, "try to open websocket at " + wss + "failed.The reason is " + reason, null));
            });

            websocketClient.OnError += onError;
            return tcs.Task;
        }

        /// <summary>
        /// 创建对话
        /// </summary>
        /// <param name="conversation">对话</param>
        /// <param name="isUnique">是否创建唯一对话，当 isUnique 为 true 时，如果当前已经有相同成员的对话存在则返回该对话，否则会创建新的对话。该值默认为 false。</param>
        /// <returns></returns>
        public Task CreateConversationAsync(AVIMConversation conversation,bool isUnique)
        {
            var cmd = new ConversationCommand()
                .Members(conversation.MemberIds)
                .Transient(conversation.IsTransient)
                .Unique(isUnique)
                .Option("start")
                .AppId(AVClient.ApplicationId)
                .PeerId(clientId);

            return AVIMClient.AVCommandRunner.RunCommandAsync(cmd);
        }

        private static readonly string[] assemblyNames = {
            "LeanMessage.Phone","LeanMessage.WinRT","LeanMessage.NetFx45","LeanMessage.iOS","LeanMessage.Android","LeanMessage.Unity"
        };
        private static Type GetAVType(string name)
        {
            foreach (var assembly in assemblyNames)
            {
                var typeName = string.Format("LeanMessage.{0}, {1}", name, assembly);
                Type type = Type.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        static AVIMClient()
        {
            Type platformHookType = GetAVType("AVIMPlatformHooks");
            if (platformHookType == null)
            {
                throw new InvalidOperationException("You must include a reference to a platform-specific LeanCloud library.");
            }
            platformHooks = Activator.CreateInstance(platformHookType) as IAVIMPlatformHooks;
            commandRunner = new AVIMCommandRunner(platformHooks.WebSocketClient);
        }

        internal static System.Version Version
        {
            get
            {
                AssemblyName assemblyName = new AssemblyName(typeof(AVIMClient).GetTypeInfo().Assembly.FullName);
                return assemblyName.Version;
            }
        }
    }
}
