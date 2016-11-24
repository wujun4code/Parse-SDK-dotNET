using LeanCloud;
using LeanCloud.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeanMessage.Internal
{
    internal class AVRouterController : IAVRouterController
    {
        const string routerUrl = "http://router.g0.push.leancloud.cn/v1/route?appId={0}&secure=1";

        public Task<RouterState> GetAsync(CancellationToken cancellationToken)
        {
           return readCache(cancellationToken).OnSuccess(_ => 
            {
                var task = Task.FromResult<RouterState>(_.Result);

                if (_.Result == null)
                {
                    task = fromCloud(cancellationToken);
                }

                return task;
            }).Unwrap();
        }

        Task<RouterState> readCache(CancellationToken cancellationToken)
        {
            try
            {
                var cache = AVClient.DecodeQueryString(AVClient.ApplicationSettings["RouterState"] as string);

                var routerState = new RouterState()
                {
                    groupId = cache["groupId"] as string,
                    server = cache["server"] as string,
                    secondary = cache["secondary"] as string,
                    ttl = long.Parse(cache["ttl"]),
                };

                return Task.FromResult<RouterState>(routerState);


            }
            catch
            {
                return Task.FromResult<RouterState>(null);
            }
        }
        Task<RouterState> fromCloud(CancellationToken cancellationToken)
        {
            string url = string.Format(routerUrl, AVClient.ApplicationId);
            return AVClient.RequestAsync(uri: new Uri(url),
                method: "GET",
                headers: null,
                data: null,
                contentType: "",
                cancellationToken: CancellationToken.None).ContinueWith<RouterState>(t =>
                {
                    var httpStatus = (int)t.Result.Item1;
                    if (httpStatus != 200)
                    {
                        throw new AVException(AVException.ErrorCode.ConnectionFailed, "can not reach router.", null);
                    }
                    try
                    {
                        var result = t.Result.Item2;

                        var routerState = AVClient.DeserializeJsonString(result);
                        var expire = DateTime.Now.AddSeconds(long.Parse(routerState["ttl"].ToString()));
                        routerState["expire"] = expire.UnixTimeStampSeconds();

                        AVClient.ApplicationSettings["RouterState"] = Json.Encode(routerState);

                        var routerStateObj = new RouterState()
                        {
                            groupId = routerState["groupId"] as string,
                            server = routerState["server"] as string,
                            secondary = routerState["secondary"] as string,
                            ttl = long.Parse(routerState["ttl"].ToString()),
                        }; 

                        return routerStateObj;
                    }
                    catch (Exception exception)
                    {
                        return null;
                    }

                });
        }
    }
}
