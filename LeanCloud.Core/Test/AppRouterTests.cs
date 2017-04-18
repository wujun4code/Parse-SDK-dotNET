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
        public void TestGetInitialAppRouter()
        {
            var state = AppRouterState.GetInitial("Abcdefghijklmn", AVClient.Configuration.AVRegion.Public_CN);
            Assert.AreEqual(state.ApiServer, "abcdefgh.api.lncld.net");
        }

        [Test]
        public Task TestQueryAppRouterAsync()
        {
            return AVPlugins.Instance.AppRouterController.QueryAsync(CancellationToken.None).ContinueWith(t =>
            {
                Assert.IsFalse(t.IsCanceled);
                Assert.IsFalse(t.IsFaulted);

                AppRouterState state = t.Result;
                Assert.IsTrue(state.TTL != 0);
                Assert.IsNotNull(state.ApiServer);
                Assert.IsNotEmpty(state.ApiServer);
                Assert.IsNotNull(state.EngineServer);
                Assert.IsNotEmpty(state.EngineServer);
                Assert.IsNotNull(state.PushServer);
                Assert.IsNotEmpty(state.PushServer);
                Assert.IsNotNull(state.RealtimeRouterServer);
                Assert.IsNotEmpty(state.RealtimeRouterServer);
                Assert.IsNotNull(state.StatsServer);
                Assert.IsNotEmpty(state.StatsServer);
                Assert.AreEqual(state.Source, "network");
            });
        }

        [Test]
        public void TestGetAppRouter()
        {
            var state = AVPlugins.Instance.AppRouterController.Get();
            Assert.IsNotNull(state);
            Assert.AreEqual(state.Source, "initial");
        }

    }
}
