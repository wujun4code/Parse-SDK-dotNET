using System;

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
        public string source { get; internal set; }

        public DateTime fetchedAt { get; internal set; }

        private static object mutex = new object();

        public AppRouterState()
        {
            fetchedAt = DateTime.Now;
        }

        /// <summary>
        /// Is this app router state expired.
        /// </summary>
        public bool isExpired()
        {
            return DateTime.Now > fetchedAt + TimeSpan.FromSeconds(ttl);
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
                        ttl = -1,
                        apiServer = String.Format("{0}.api.lncld.net", prefix),
                        engineServer = String.Format("{0}.engine.lncld.net", prefix),
                        pushServer =String.Format("{0}.push.lncld.net", prefix),
                        rtmRouterServer = String.Format("{0}.rtm.lncld.net", prefix),
                        statsServer = String.Format("{0}.stats.lncld.net", prefix),
                        source = "initial",
                    };
                case AVClient.Configuration.AVRegion.Public_US:
                    return new AppRouterState()
                    {
                        ttl = -1,
                        apiServer = "us-api.leancloud.cn",
                        engineServer = "us-api.leancloud.cn",
                        pushServer = "us-api.leancloud.cn",
                        rtmRouterServer = "router-a0-push.leancloud.cn",
                        statsServer = "us-api.leancloud.cn",
                        source = "initial",
                    };
                case AVClient.Configuration.AVRegion.Vendor_Tencent:
                    return new AppRouterState()
                    {
                        ttl = -1,
                        apiServer = "e1-api.leancloud.cn",
                        engineServer = "e1-api.leancloud.cn",
                        pushServer = "e1-api.leancloud.cn",
                        rtmRouterServer = "router-q0-push.leancloud.cn",
                        statsServer = "e1-api.leancloud.cn",
                        source = "initial",
                    };
                default:
                    throw new AVException(AVException.ErrorCode.OtherCause, "invalid region");
            }
        }

    }
}