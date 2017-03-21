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
        public Task TestQueryAppRouterAsync()
        {
            return AVPlugins.Instance.AppRouterController.QueryAsync(CancellationToken.None).ContinueWith(t =>
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
                Assert.AreEqual(state.source, "network");
            });
        }

        [Test]
        public void TestGetAppRouter()
        {
            var state = AVPlugins.Instance.AppRouterController.Get();
            Assert.IsNotNull(state);
            Assert.AreEqual(state.source, "initial");
        }

    }
}
