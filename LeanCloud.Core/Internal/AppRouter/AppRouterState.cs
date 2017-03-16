using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanCloud.Core.Internal
{
    public class AppRouterState
    {
        public long ttl { get; internal set; }
        public string apiServer { get; internal set; }
        public string engineServer { get; internal set; }
        public string pushServer { get; internal set; }
        public string rtmRouterServer { get; internal set; }
        public string statsServer { get; internal set; }

        private static object mutex = new object();

        private static AppRouterState _regionCNInitialState;
        /// <summary>
        /// The CN region's initial AppRouterState instance
        /// </summary>
        public static AppRouterState regionCNInitialState
        {
            get
            {
                lock (mutex)
                {
                    if (_regionCNInitialState == null)
                    {
                        _regionCNInitialState = new AppRouterState()
                        {
                            ttl = -1,
                            apiServer = "api.leancloud.cn",
                            engineServer = "api.leancloud.cn",
                            pushServer = "api.leancloud.cn",
                            rtmRouterServer = "router-g0-push.leancloud.cn",
                            statsServer = "api.leancloud.cn",
                        };
                    }
                    return _regionCNInitialState;
                };
            }
        }

        private static AppRouterState _regionUSInitialState;
        /// <summary>
        /// The US region's initial AppRouterState instance
        /// </summary>
        public static AppRouterState regionUSInitialState
        {
            get
            {
                lock (mutex)
                {
                    if (_regionUSInitialState == null)
                    {
                        _regionUSInitialState = new AppRouterState()
                        {
                            ttl = -1,
                            apiServer = "us-api.leancloud.cn",
                            engineServer = "us-api.leancloud.cn",
                            pushServer = "us-api.leancloud.cn",
                            rtmRouterServer = "router-a0-push.leancloud.cn",
                            statsServer = "us-api.leancloud.cn",
                        };
                    }
                    return _regionUSInitialState;
                };
            }
        }


        private static AppRouterState _regionTABInitialState;
        /// <summary>
        /// The TAB region's initial AppRouterState instance
        /// </summary>
        public static AppRouterState regionTABInitialState
        {
            get
            {
                lock (mutex)
                {
                    if (_regionTABInitialState == null)
                    {
                        _regionTABInitialState = new AppRouterState()
                        {
                            ttl = -1,
                            apiServer = "e1-api.leancloud.cn",
                            engineServer = "e1-api.leancloud.cn",
                            pushServer = "e1-api.leancloud.cn",
                            rtmRouterServer = "router-q0-push.leancloud.cn",
                            statsServer = "e1-api.leancloud.cn",
                        };
                    }
                    return _regionTABInitialState;
                };
            }
        }
    }
}