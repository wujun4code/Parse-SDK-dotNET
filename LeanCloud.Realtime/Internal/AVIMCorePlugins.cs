﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanCloud.Realtime.Internal
{
    internal class AVIMCorePlugins
    {
        private static readonly AVIMCorePlugins instance = new AVIMCorePlugins();
        public static AVIMCorePlugins Instance
        {
            get
            {
                return instance;
            }
        }

        private readonly object mutex = new object();

        private IAVRouterController routerController;
        public IAVRouterController RouterController
        {
            get
            {
                lock (mutex)
                {
                    routerController = routerController ?? new AVRouterController();
                    return routerController;
                }
            }
            internal set
            {
                lock (mutex)
                {
                    routerController = value;
                }
            }
        }

        private IWebSocketClient webSocketController;

        public IWebSocketClient WebSocketController
        {
            get
            {
                lock (mutex)
                {
#if !UNITY
                    webSocketController = webSocketController ?? new WebSocketClient();
#endif
#if UNITY 
                    if (webSocketController == null)
                        throw new MissingMemberException("AVIMCorePlugins", "IWebSocketClient");
#endif
                    return webSocketController;
                }
            }
            internal set
            {
                lock (mutex)
                {
                    webSocketController = value;
                }
            }
        }
        private IAVIMCommandRunner imCommandRunner;

        public IAVIMCommandRunner IMCommandRunner
        {
            get
            {
                lock (mutex)
                {
                    imCommandRunner = imCommandRunner ?? new AVIMCommandRunner(this.WebSocketController);
                    return imCommandRunner;
                }
            }
        }


    }
}
