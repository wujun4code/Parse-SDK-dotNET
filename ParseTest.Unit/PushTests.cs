using Moq;
using NUnit.Framework;
using LeanCloud;
using LeanCloud.Internal;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloudTest {
  [TestFixture]
  public class PushTests {
    private IAVPushController GetMockedPushController(IPushState expectedPushState) {
      Mock<IAVPushController> mockedController = new Mock<IAVPushController>(MockBehavior.Strict);

      mockedController.Setup(
        obj => obj.SendPushNotificationAsync(
          It.Is<IPushState>(s => s.Equals(expectedPushState)),
          It.IsAny<string>(),
          It.IsAny<CancellationToken>()
        )
      ).Returns(Task.FromResult(false));

      return mockedController.Object;
    }

    private IAVPushChannelsController GetMockedPushChannelsController(IEnumerable<string> channels) {
      Mock<IAVPushChannelsController> mockedChannelsController = new Mock<IAVPushChannelsController>(MockBehavior.Strict);

      mockedChannelsController.Setup(
        obj => obj.SubscribeAsync(
          It.Is<IEnumerable<string>>(it => it.CollectionsEqual(channels)),
          It.IsAny<CancellationToken>()
        )
      ).Returns(Task.FromResult(false));

      mockedChannelsController.Setup(
        obj => obj.UnsubscribeAsync(
          It.Is<IEnumerable<string>>(it => it.CollectionsEqual(channels)),
          It.IsAny<CancellationToken>()
        )
      ).Returns(Task.FromResult(false));

      return mockedChannelsController.Object;
    }

    [Test]
    [AsyncStateMachine(typeof(PushTests))]
    public Task TestSendPush() {
      MutablePushState state = new MutablePushState {
        Query = AVInstallation.Query
      };

      AVPush thePush = new AVPush();
      AVCorePlugins.Instance.PushController = GetMockedPushController(state);

      thePush.Alert = "Alert";
      state.Alert = "Alert";

      return thePush.SendAsync().ContinueWith(t => {
        Assert.True(t.IsCompleted);
        Assert.False(t.IsFaulted);

        thePush.Channels = new List<string> { { "channel" } };
        state.Channels = new List<string> { { "channel" } };

        return thePush.SendAsync();
      }).Unwrap().ContinueWith(t => {
        Assert.True(t.IsCompleted);
        Assert.False(t.IsFaulted);

        AVQuery<AVInstallation> query = new AVQuery<AVInstallation>("aClass");
        thePush.Query = query;
        state.Query = query;

        return thePush.SendAsync();
      }).Unwrap().ContinueWith(t => {
        Assert.True(t.IsCompleted);
        Assert.False(t.IsFaulted);

        AVCorePlugins.Instance.PushController = null;
      });
    }

    [Test]
    [AsyncStateMachine(typeof(PushTests))]
    public Task TestSubscribe() {
      List<string> channels = new List<string>();
      AVCorePlugins.Instance.PushChannelsController = GetMockedPushChannelsController(channels);

      channels.Add("test");
      return AVPush.SubscribeAsync("test").ContinueWith(t => {
        Assert.IsTrue(t.IsCompleted);
        Assert.IsFalse(t.IsFaulted);

        return AVPush.SubscribeAsync(new List<string> { { "test" } });
      }).Unwrap().ContinueWith(t => {
        Assert.IsTrue(t.IsCompleted);
        Assert.IsFalse(t.IsFaulted);

        CancellationTokenSource cts = new CancellationTokenSource();
        return AVPush.SubscribeAsync(new List<string> { { "test" } }, cts.Token);
      }).Unwrap().ContinueWith(t => {
        Assert.IsTrue(t.IsCompleted);
        Assert.IsFalse(t.IsFaulted);

        AVCorePlugins.Instance.PushChannelsController = null;
      });
    }

    [Test]
    [AsyncStateMachine(typeof(PushTests))]
    public Task TestUnsubscribe() {
      List<string> channels = new List<string>();
      AVCorePlugins.Instance.PushChannelsController = GetMockedPushChannelsController(channels);

      channels.Add("test");
      return AVPush.UnsubscribeAsync("test").ContinueWith(t => {
        Assert.IsTrue(t.IsCompleted);
        Assert.IsFalse(t.IsFaulted);

        return AVPush.UnsubscribeAsync(new List<string> { { "test" } });
      }).ContinueWith(t => {
        Assert.IsTrue(t.IsCompleted);
        Assert.IsFalse(t.IsFaulted);

        CancellationTokenSource cts = new CancellationTokenSource();
        return AVPush.UnsubscribeAsync(new List<string> { { "test" } }, cts.Token);
      }).ContinueWith(t => {
        Assert.IsTrue(t.IsCompleted);
        Assert.IsFalse(t.IsFaulted);

        AVCorePlugins.Instance.PushChannelsController = null;
      });
    }
  }
}
