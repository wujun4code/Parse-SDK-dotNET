﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud.Realtime.Internal
{
    interface IAVRouterController
    {
        Task<PushRouterState> GetAsync(CancellationToken cancellationToken);
    }
}
