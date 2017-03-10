using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using LeanCloud.Storage.Internal;

namespace ParseTest
{
    class HttpClientTests
    {
        [Test]
        public Task TestUserAgent()
        {
            HttpRequest request = new HttpRequest();
            request.Uri = new Uri("http://httpbin.org/user-agent");
            request.Method = "GET";
            HttpClient client = new HttpClient();
            return client.ExecuteAsync(request, null, null, new CancellationToken()).ContinueWith(t => {
                Assert.IsFalse(t.IsFaulted);
                Assert.IsFalse(t.IsCanceled);
                string body = t.Result.Item2;
                Assert.IsTrue(body.Contains("LeanCloud-dotNet-SDK/"));
            });
        }
    }
}
