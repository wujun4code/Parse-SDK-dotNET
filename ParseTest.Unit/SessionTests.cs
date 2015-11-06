using LeanCloud;
using LeanCloud.Internal;
using NUnit.Framework;
using Moq;
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloudTest {
  [TestFixture]
  public class SessionTests {
    [SetUp]
    public void SetUp() {
      AVObject.RegisterSubclass<AVSession>();
      AVObject.RegisterSubclass<AVUser>();
    }

    [TearDown]
    public void TearDown() {
      AVCorePlugins.Instance.SessionController = null;
      AVCorePlugins.Instance.CurrentUserController = null;
      AVObject.UnregisterSubclass("_Session");
      AVObject.UnregisterSubclass("_User");
    }

    [Test]
    public void TestGetSessionQuery() {
      Assert.IsInstanceOf<AVQuery<AVSession>>(AVSession.Query);
    }

    [Test]
    public void TestGetSessionToken() {
      IObjectState state = new MutableObjectState {
        ServerData = new Dictionary<string, object>() {
          { "sessionToken", "llaKcolnu" }
        }
      };
      AVSession session = AVObject.FromState<AVSession>(state, "_Session");
      Assert.NotNull(session);
      Assert.AreEqual("llaKcolnu", session.SessionToken);
    }

    [Test]
    public void TestIsRevocableSessionToken() {
      Assert.True(AVSession.IsRevocableSessionToken("r:session"));
      Assert.True(AVSession.IsRevocableSessionToken("r:session:r:"));
      Assert.True(AVSession.IsRevocableSessionToken("session:r:"));
      Assert.False(AVSession.IsRevocableSessionToken("session:s:d:r"));
      Assert.False(AVSession.IsRevocableSessionToken("s:ession:s:d:r"));
      Assert.False(AVSession.IsRevocableSessionToken(""));
    }

    [Test]
    [AsyncStateMachine(typeof(SessionTests))]
    public Task TestGetCurrentSession() {
      IObjectState sessionState = new MutableObjectState {
        ServerData = new Dictionary<string, object>() {
          { "sessionToken", "newllaKcolnu" }
        }
      };
      var mockController = new Mock<IAVSessionController>();
      mockController.Setup(obj => obj.GetSessionAsync(It.IsAny<string>(),
          It.IsAny<CancellationToken>())).Returns(Task.FromResult(sessionState));

      IObjectState userState = new MutableObjectState {
        ServerData = new Dictionary<string, object>() {
          { "sessionToken", "llaKcolnu" }
        }
      };
      AVUser user = AVObject.FromState<AVUser>(userState, "_User");
      var mockCurrentUserController = new Mock<IAVCurrentUserController>();
      mockCurrentUserController.Setup(obj => obj.GetAsync(It.IsAny<CancellationToken>()))
          .Returns(Task.FromResult(user));

      AVCorePlugins.Instance.SessionController = mockController.Object;
      AVCorePlugins.Instance.CurrentUserController = mockCurrentUserController.Object;

      return AVSession.GetCurrentSessionAsync().ContinueWith(t => {
        Assert.False(t.IsFaulted);
        Assert.False(t.IsCanceled);
        mockController.Verify(obj => obj.GetSessionAsync(It.Is<string>(sessionToken => sessionToken == "llaKcolnu"),
            It.IsAny<CancellationToken>()), Times.Exactly(1));

        var session = t.Result;
        Assert.AreEqual("newllaKcolnu", session.SessionToken);
      });
    }

    [Test]
    [AsyncStateMachine(typeof(SessionTests))]
    public Task TestGetCurrentSessionWithNoCurrentUser() {
      var mockController = new Mock<IAVSessionController>();
      var mockCurrentUserController = new Mock<IAVCurrentUserController>();
      AVCorePlugins.Instance.SessionController = mockController.Object;
      AVCorePlugins.Instance.CurrentUserController = mockCurrentUserController.Object;

      return AVSession.GetCurrentSessionAsync().ContinueWith(t => {
        Assert.False(t.IsFaulted);
        Assert.False(t.IsCanceled);
        Assert.Null(t.Result);
      });
    }

    [Test]
    [AsyncStateMachine(typeof(SessionTests))]
    public Task TestRevoke() {
      var mockController = new Mock<IAVSessionController>();
      AVCorePlugins.Instance.SessionController = mockController.Object;

      CancellationTokenSource source = new CancellationTokenSource();
      return AVSession.RevokeAsync("r:someSession", source.Token).ContinueWith(t => {
        Assert.False(t.IsFaulted);
        Assert.False(t.IsCanceled);
        mockController.Verify(obj => obj.RevokeAsync(It.Is<string>(sessionToken => sessionToken == "r:someSession"),
            source.Token), Times.Exactly(1));
      });
    }

    [Test]
    [AsyncStateMachine(typeof(SessionTests))]
    public Task TestUpgradeToRevocableSession() {
      IObjectState state = new MutableObjectState {
        ServerData = new Dictionary<string, object>() {
          { "sessionToken", "llaKcolnu" }
        }
      };
      var mockController = new Mock<IAVSessionController>();
      mockController.Setup(obj => obj.UpgradeToRevocableSessionAsync(It.IsAny<string>(),
          It.IsAny<CancellationToken>())).Returns(Task.FromResult(state));

      var mockCurrentUserController = new Mock<IAVCurrentUserController>();
      AVCorePlugins.Instance.SessionController = mockController.Object;
      AVCorePlugins.Instance.CurrentUserController = mockCurrentUserController.Object;

      CancellationTokenSource source = new CancellationTokenSource();
      return AVSession.UpgradeToRevocableSessionAsync("someSession", source.Token).ContinueWith(t => {
        Assert.False(t.IsFaulted);
        Assert.False(t.IsCanceled);
        mockController.Verify(obj => obj.UpgradeToRevocableSessionAsync(It.Is<string>(sessionToken => sessionToken == "someSession"),
            source.Token), Times.Exactly(1));

        Assert.AreEqual("llaKcolnu", t.Result);
      });
    }
  }
}
