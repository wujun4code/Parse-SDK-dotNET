using System;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeanCloud.Storage.Internal;

namespace LeanCloud.Core.Internal
{
    public class AppRouterController : IAppRouterController
    {
        private AppRouterState currentState;
        private object mutex = new object();

        /// <summary>
        /// Get current app's router state
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public AppRouterState Get()
        {
            AppRouterState state;
            lock (mutex)
            {
                if (currentState != null)
                {
                    state = currentState;
                }
                var appId = AVClient.CurrentConfiguration.ApplicationId;
                var region = AVClient.CurrentConfiguration.Region;
                state = AppRouterState.GetInitial(appId, region);
            }

            if (AVClient.CurrentConfiguration.Region != AVClient.Configuration.AVRegion.Public_US)
            {
                // don't refresh app router in US region.
                return state;
            }

            if (state.isExpired())
            {
                lock (mutex)
                {
                    state.fetchedAt = DateTime.Now + TimeSpan.FromMinutes(10);
                }
                Task.Factory.StartNew(RefreshAsync);
            }
            return state;
        }

        public Task RefreshAsync()
        {
            return QueryAsync(CancellationToken.None).ContinueWith(t =>
            {
                if (!t.IsFaulted && !t.IsCanceled)
                {
                    lock (mutex)
                    {
                        currentState = t.Result;
                    }
                }
            });
        }

        public Task<AppRouterState> QueryAsync(CancellationToken cancellationToken)
        {
            string appId = AVClient.CurrentConfiguration.ApplicationId;
            string url = string.Format("https://app-router.leancloud.cn/2/route?appId={0}", appId);
            HttpRequest request = new HttpRequest()
            {
                Uri = new Uri(url),
                Method = "GET",
            };
            return AVPlugins.Instance.HttpClient.ExecuteAsync(request, null, null, cancellationToken).ContinueWith(t =>
               {
                   if (t.Result.Item1 != HttpStatusCode.OK)
                   {
                       throw new AVException(AVException.ErrorCode.ConnectionFailed, "can not reach router.", null);
                   }

                   var result = Json.Parse(t.Result.Item2) as IDictionary<String, Object>;
                   return ParseAppRouterState(result);
               });
        }

        private static AppRouterState ParseAppRouterState(IDictionary<string, object> jsonObj)
        {
            var state = new AppRouterState()
            {
                ttl = (long)jsonObj["ttl"],
                statsServer = jsonObj["stats_server"] as string,
                rtmRouterServer = jsonObj["rtm_router_server"] as string,
                pushServer = jsonObj["push_server"] as string,
                engineServer = jsonObj["engine_server"] as string,
                apiServer = jsonObj["api_server"] as string,
                source = "network",
            };
            return state;
        }
    }
}
