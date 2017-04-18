using System;

namespace LeanCloud.Core.Internal
{
    public class AppRouterState
    {
        public long TTL { get; internal set; }
        public string ApiServer { get; internal set; }
        public string EngineServer { get; internal set; }
        public string PushServer { get; internal set; }
        public string RealtimeRouterServer { get; internal set; }
        public string StatsServer { get; internal set; }
        public string Source { get; internal set; }

        public DateTime FetchedAt { get; internal set; }

        private static object mutex = new object();

        public AppRouterState()
        {
            FetchedAt = DateTime.Now;
        }

        /// <summary>
        /// Is this app router state expired.
        /// </summary>
        public bool isExpired()
        {
            return DateTime.Now > FetchedAt + TimeSpan.FromSeconds(TTL);
        }

        /// <summary>
        /// Get the initial usable router state
        /// </summary>
        /// <param name="appId">Current app's appId</param>
        /// <param name="region">Current app's region</param>
        /// <returns>Initial app router state</returns>
        public static AppRouterState GetInitial(string appId,  AVClient.Configuration.AVRegion region)
        {
            switch (region)
            {
                case AVClient.Configuration.AVRegion.Public_CN:
                    var prefix = appId.Substring(0, 8).ToLower();
                    return new AppRouterState()
                    {
                        TTL = -1,
                        ApiServer = String.Format("{0}.api.lncld.net", prefix),
                        EngineServer = String.Format("{0}.engine.lncld.net", prefix),
                        PushServer =String.Format("{0}.push.lncld.net", prefix),
                        RealtimeRouterServer = String.Format("{0}.rtm.lncld.net", prefix),
                        StatsServer = String.Format("{0}.stats.lncld.net", prefix),
                        Source = "initial",
                    };
                case AVClient.Configuration.AVRegion.Public_US:
                    return new AppRouterState()
                    {
                        TTL = -1,
                        ApiServer = "us-api.leancloud.cn",
                        EngineServer = "us-api.leancloud.cn",
                        PushServer = "us-api.leancloud.cn",
                        RealtimeRouterServer = "router-a0-push.leancloud.cn",
                        StatsServer = "us-api.leancloud.cn",
                        Source = "initial",
                    };
                case AVClient.Configuration.AVRegion.Vendor_Tencent:
                    return new AppRouterState()
                    {
                        TTL = -1,
                        ApiServer = "e1-api.leancloud.cn",
                        EngineServer = "e1-api.leancloud.cn",
                        PushServer = "e1-api.leancloud.cn",
                        RealtimeRouterServer = "router-q0-push.leancloud.cn",
                        StatsServer = "e1-api.leancloud.cn",
                        Source = "initial",
                    };
                default:
                    throw new AVException(AVException.ErrorCode.OtherCause, "invalid region");
            }
        }

    }
}