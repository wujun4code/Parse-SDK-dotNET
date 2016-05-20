using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LeanMessage.Internal
{
    interface IAVRouterController
    {
        Task<RouterState> GetAsync(CancellationToken cancellationToken);
    }
}
