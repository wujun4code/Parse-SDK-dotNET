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
        private const string cacheKey = "LeanCloudAppRouterState";
        private AppRouterState currentState;
        private object mutex = new object();
        private bool shouldStopRefresh = false;

        /// <summary>
        /// Get current app's router state
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<AppRouterState> GetAsync(CancellationToken cancellationToken)
        {
            lock (mutex)
            {
                if (currentState != null)
                {
                    return Task.FromResult(currentState);
                }
                switch (AVClient.CurrentConfiguration.Region)
                {
                    case AVClient.Configuration.AVRegion.Public_US:
                        return Task.FromResult(AppRouterState.regionUSInitialState);
                    case AVClient.Configuration.AVRegion.Vendor_Tencent:
                        return Task.FromResult(AppRouterState.regionTABInitialState);
                    case AVClient.Configuration.AVRegion.Public_CN:
                        return Task.FromResult(AppRouterState.regionUSInitialState);
                    default:
                        // TODO (asaka): more suitable exception description
                        throw new AVException(0, "SDK is not initailized");
                }
            }
        }

        public Task StartRefreshAsync(TimeSpan delay)
        {
            if (shouldStopRefresh)
            {
                return Task.FromResult(true);
            }
            return Task.Delay(delay).ContinueWith(t =>
            {
                return QueryAsync(CancellationToken.None);
            }).Unwrap().ContinueWith(t =>
            {
                if (!t.IsFaulted && !t.IsCanceled)
                {
                    lock (mutex)
                    {
                        currentState = t.Result;
                    }
                    delay = TimeSpan.FromSeconds((int)t.Result.ttl);
                }
                else
                {
                    delay = TimeSpan.FromSeconds(600);  // default exception retry delay
                }
                return StartRefreshAsync(delay);
            }).Unwrap();
        }

        public void StopRefresh()
        {
            shouldStopRefresh = true;
        }

        private Task<AppRouterState> QueryAsync(CancellationToken cancellationToken)
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

                   AVPlugins.Instance.StorageController.LoadAsync().OnSuccess(storage => storage.Result.AddAsync(cacheKey, t.Result.Item2));

                   var result = Json.Parse(t.Result.Item2) as IDictionary<String, Object>;
                   return ParseAppRouterState(result);
               });
        }

        private static AppRouterState ParseAppRouterState(IDictionary<string, object> jsonObj)
        {
            return new AppRouterState()
            {
                ttl = (long)jsonObj["ttl"],
                statsServer = jsonObj["stats_server"] as string,
                rtmRouterServer = jsonObj["rtm_router_server"] as string,
                pushServer = jsonObj["push_server"] as string,
                engineServer = jsonObj["engine_server"] as string,
                apiServer = jsonObj["api_server"] as string,
            };

        }
    }
}
