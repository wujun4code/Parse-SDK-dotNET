using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanCloud.Core.Internal
{
    interface IAppRouterController
    {
        Task<AppRouterState> GetAsync(string routeRoot);
    }
}
