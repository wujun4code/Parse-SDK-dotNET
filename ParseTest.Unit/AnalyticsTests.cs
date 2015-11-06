using Moq;
using NUnit.Framework;
using LeanCloud;
using LeanCloud.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloudTest {
  [TestFixture]
  public class AnalyticsTests {
    [TearDown]
    public void TearDown() {
      AVCorePlugins.Instance.AnalyticsController = null;
      AVCorePlugins.Instance.CurrentUserController = null;
    }

    [Test]
    [AsyncStateMachine(typeof(AnalyticsTests))]
    public Task TestTrackEvent() {
      var mockController = new Mock<IAVAnalyticsController>();
      var mockCurrentUserController = new Mock<IAVCurrentUserController>();
      AVCorePlugins.Instance.AnalyticsController = mockController.Object;
      AVCorePlugins.Instance.CurrentUserController = mockCurrentUserController.Object;

      return AVAnalytics.TrackEventAsync("SomeEvent").ContinueWith(t => {
        Assert.IsFalse(t.IsFaulted);
        Assert.IsFalse(t.IsCanceled);
        mockController.Verify(obj => obj.TrackEventAsync(It.Is<string>(eventName => eventName == "SomeEvent"),
            It.Is<IDictionary<string, string>>(dict => dict == null),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Exactly(1));
      });
    }

    [Test]
    [AsyncStateMachine(typeof(AnalyticsTests))]
    public Task TestTrackEventWithDimension() {
      var mockController = new Mock<IAVAnalyticsController>();
      var mockCurrentUserController = new Mock<IAVCurrentUserController>();
      var dimensions = new Dictionary<string, string>() {
        { "facebook", "hq" }
      };
      AVCorePlugins.Instance.AnalyticsController = mockController.Object;
      AVCorePlugins.Instance.CurrentUserController = mockCurrentUserController.Object;

      return AVAnalytics.TrackEventAsync("SomeEvent", dimensions).ContinueWith(t => {
        Assert.IsFalse(t.IsFaulted);
        Assert.IsFalse(t.IsCanceled);
        mockController.Verify(obj => obj.TrackEventAsync(It.Is<string>(eventName => eventName == "SomeEvent"),
            It.Is<IDictionary<string, string>>(dict => dict != null && dict.Count == 1),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Exactly(1));
      });
    }

    [Test]
    [AsyncStateMachine(typeof(AnalyticsTests))]
    public Task TestTrackAppOpened() {
      var mockController = new Mock<IAVAnalyticsController>();
      var mockCurrentUserController = new Mock<IAVCurrentUserController>();
      AVCorePlugins.Instance.AnalyticsController = mockController.Object;
      AVCorePlugins.Instance.CurrentUserController = mockCurrentUserController.Object;

      return AVAnalytics.TrackAppOpenedAsync().ContinueWith(t => {
        Assert.IsFalse(t.IsFaulted);
        Assert.IsFalse(t.IsCanceled);
        mockController.Verify(obj => obj.TrackAppOpenedAsync(It.Is<string>(pushHash => pushHash == null),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Exactly(1));
      });
    }
  }
}
