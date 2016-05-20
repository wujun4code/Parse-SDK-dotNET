using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeanMessage.Internal
{
    internal interface IAVWebSocketCommandRunner
    {
        Task<Tuple<int, IDictionary<string, object>>> RunCommandAsync(AVWebSocketCommand command,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
