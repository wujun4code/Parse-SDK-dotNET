using System;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using NUnit.Framework;
using LeanCloud;
using LeanCloud.Core.Internal;

namespace ParseTest
{
    class AppRouterTests
    {
        [SetUp]
        public void SetUp()
        {
            string appId = ConfigurationManager.AppSettings["appId"];
            string appKey = ConfigurationManager.AppSettings["appKey"];
            AVClient.Initialize(appId, appKey);

            AVUser.LogOut();
        }

        [TearDown]
        public void TearDown()
        {
            AVPlugins.Instance.Reset();
        }

        [Test]
        public Task TestGetAppRouterAsync()
        {
            return AVPlugins.Instance.AppRouterController.GetAsync(CancellationToken.None).ContinueWith(t =>
            {
                Assert.IsFalse(t.IsCanceled);
                Assert.IsFalse(t.IsFaulted);

                AppRouterState state = t.Result;
                Assert.IsTrue(state.ttl != 0);
                Assert.IsNotNull(state.apiServer);
                Assert.IsNotEmpty(state.apiServer);
                Assert.IsNotNull(state.engineServer);
                Assert.IsNotEmpty(state.engineServer);
                Assert.IsNotNull(state.pushServer);
                Assert.IsNotEmpty(state.pushServer);
                Assert.IsNotNull(state.rtmRouterServer);
                Assert.IsNotEmpty(state.rtmRouterServer);
                Assert.IsNotNull(state.statsServer);
                Assert.IsNotEmpty(state.statsServer);
            });
        }

        [Test]
        public Task TestStartRefreshAppRouter()
        {
            return Task.Delay(3000).ContinueWith(_ =>
            {
                return AVPlugins.Instance.AppRouterController.GetAsync(CancellationToken.None).ContinueWith(t =>
                {
                    Assert.IsFalse(t.IsCanceled);
                    Assert.IsFalse(t.IsFaulted);

                    AppRouterState state = t.Result;

                    Assert.AreEqual(state.apiServer, ConfigurationManager.AppSettings["appId"].Substring(0, 8) + ".api.lncld.net");
                    AVPlugins.Instance.AppRouterController.StopRefresh();
                });
            }).Unwrap();
        }
    }
}
