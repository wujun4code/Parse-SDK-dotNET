using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeanMessage.Internal
{
    interface IWebSocketClient
    {
        Task<Tuple<int, IDictionary<string, object>>> RunCommandAsync(AVWebSocketCommand command,
            CancellationToken cancellationToken = default(CancellationToken));

        Task OpenAsync(string wss, CancellationToken cancellationToken = default(CancellationToken));

        event EventHandler<IDictionary<string, object>> OnMessage;
    }
}
