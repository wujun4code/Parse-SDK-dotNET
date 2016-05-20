using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanMessage.Internal
{
    interface IAVIMPlatformHooks
    {
        IWebSocketClient WebSocketClient { get; }

        string ua { get; }
    }
}
