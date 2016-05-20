using LeanMessage.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Websockets;
using Websockets.Net;

namespace LeanMessage.NetFx45.Internal
{
    internal class WebSocketClient : IWebSocketClient
    {
        internal readonly IWebSocketConnection connection;
        public WebSocketClient()
        {
            WebsocketConnection.Link();
            connection = WebSocketFactory.Create();
        }
        
        public event EventHandler<IDictionary<string, object>> OnMessage;

        public Task OpenAsync(string wss, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<bool>();
            connection.Open(wss);
            Action onOpend = null;
            onOpend = (() => 
            {
                connection.OnOpened -= onOpend;
                tcs.SetResult(true);
            });
            connection.OnOpened += onOpend;

            Action<string> onError = null;
            onError = ((reason) => 
            {
                connection.OnError -= onError;
                tcs.SetResult(false);
                tcs.TrySetException(new AVIMException(AVIMException.ErrorCode.FromServer, "try to open websocket at " + wss + "failed.The reason is "+reason, null));
            });

            connection.OnError += onError;
            return tcs.Task;
        }

        public Task<Tuple<int, IDictionary<string, object>>> RunCommandAsync(AVWebSocketCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}
