using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanCloud.Core.Internal
{
    internal class AppRouterState
    {
        public long ttl { get; internal set; }
        public string push_router_server { get; internal set; }
        public string api_server { get; internal set; } 
    }
}
