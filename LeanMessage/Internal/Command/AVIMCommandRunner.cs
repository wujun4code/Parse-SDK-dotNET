using LeanCloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeanMessage.Internal
{
    internal class AVIMCommandRunner : IAVIMCommandRunner
    {
        private readonly IWebSocketClient webSocketClient;
        public AVIMCommandRunner(IWebSocketClient webSocketClient)
        {
            this.webSocketClient = webSocketClient;
        }

        public Task<Tuple<int, IDictionary<string, object>>> RunCommandAsync(AVIMCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            command = command.IDlize();
            var tcs = new TaskCompletionSource<Tuple<int, IDictionary<string, object>>>();
            var requestString = command.EncodeJsonString();
            webSocketClient.Send(requestString);
            var requestJson = command.Encode();

            Action<string> onMessage = null;
            onMessage = (response) => 
            {
                var responseJson = AVClient.DeserializeJsonString(response);
                if (requestJson["i"].ToString() == responseJson["i"].ToString())
                {
                    var result = new Tuple<int, IDictionary<string, object>>(-1, responseJson);
                    if (responseJson.Keys.Contains("code"))
                    {
                        var errorCode = int.Parse(responseJson["code"].ToString());
                        var reason = string.Empty;
                        int appCode = 0;
                        //result = new Tuple<int, IDictionary<string, object>>(errorCode, responseJson);
                        if (responseJson.Keys.Contains("reason"))
                        {
                            reason = responseJson["reason"].ToString();
                        }
                        if (responseJson.Keys.Contains("appCode"))
                        {
                            appCode = int.Parse(responseJson["appCode"].ToString());
                        }
                        tcs.SetException(new AVIMException(errorCode, appCode, reason, null));
                    }
                    tcs.SetResult(result);
                    webSocketClient.OnMessage -= onMessage;
                }
            };
            webSocketClient.OnMessage += onMessage;
            return tcs.Task;
        }
    }
}
