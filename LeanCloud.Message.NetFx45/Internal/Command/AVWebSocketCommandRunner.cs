using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeanMessage.Internal
{
    internal class AVWebSocketCommandRunner : IAVWebSocketCommandRunner
    {

        private readonly IWebSocketClient webSocketClient;
        public AVWebSocketCommandRunner(IWebSocketClient webSocketClient)
        {
            this.webSocketClient = webSocketClient;
        }

        public Task<Tuple<int, IDictionary<string, object>>> RunCommandAsync(AVWebSocketCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.webSocketClient.RunCommandAsync(command, cancellationToken);
        }
    }
}
