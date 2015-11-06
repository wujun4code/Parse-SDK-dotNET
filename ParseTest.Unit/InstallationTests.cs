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
  public class InstallationTests {
    [SetUp]
    public void SetUp() {
      AVObject.RegisterSubclass<AVInstallation>();
    }

    [TearDown]
    public void TearDown() {
      AVCorePlugins.Instance.ObjectController = null;
      AVCorePlugins.Instance.CurrentInstallationController = null;
      AVCorePlugins.Instance.CurrentUserController = null;
      AVObject.UnregisterSubclass("_Installation");
    }

    [Test]
    public void TestGetInstallationQuery() {
      Assert.IsInstanceOf<AVQuery<AVInstallation>>(AVInstallation.Query);
    }

    [Test]
    public void TestInstallationIdGetterSetter() {
      var guid = Guid.NewGuid();
      IObjectState state = new MutableObjectState {
        ServerData = new Dictionary<string, object>() {
          { "installationId", guid.ToString() }
        }
      };
      AVInstallation installation = AVObject.FromState<AVInstallation>(state, "_Installation");
      Assert.NotNull(installation);
      Assert.AreEqual(guid, installation.InstallationId);

      var newGuid = Guid.NewGuid();
      Assert.Throws<InvalidOperationException>(() => installation.InstallationId = newGuid);
      installation.SetIfDifferent<string>("installationId", newGuid.ToString());
      Assert.AreEqual(newGuid, installation.InstallationId);
    }

    [Test]
    public void TestDeviceTypeGetterSetter() {
      IObjectState state = new MutableObjectState {
        ServerData = new Dictionary<string, object>() {
          { "deviceType", "parseOS" }
        }
      };
      AVInstallation installation = AVObject.FromState<AVInstallation>(state, "_Installation");
      Assert.NotNull(installation);
      Assert.AreEqual("parseOS", installation.DeviceType);

      Assert.Throws<InvalidOperationException>(() => installation.DeviceType = "gogoOS");
      installation.SetIfDifferent("deviceType", "gogoOS");
      Assert.AreEqual("gogoOS", installation.DeviceType);
    }

    [Test]
    public void TestAppNameGetterSetter() {
      IObjectState state = new MutableObjectState {
        ServerData = new Dictionary<string, object>() {
          { "appName", "parseApp" }
        }
      };
      AVInstallation installation = AVObject.FromState<AVInstallation>(state, "_Installation");
      Assert.NotNull(installation);
      Assert.AreEqual("parseApp", installation.AppName);

      Assert.Throws<InvalidOperationException>(() => installation.AppName = "gogoApp");
      installation.SetIfDifferent("appName", "gogoApp");
      Assert.AreEqual("gogoApp", installation.AppName);
    }

    [Test]
    public void TestAppVersionGetterSetter() {
      IObjectState state = new MutableObjectState {
        ServerData = new Dictionary<string, object>() {
          { "appVersion", "1.2.3" }
        }
      };
      AVInstallation installation = AVObject.FromState<AVInstallation>(state, "_Installation");
      Assert.NotNull(installation);
      Assert.AreEqual("1.2.3", installation.AppVersion);

      Assert.Throws<InvalidOperationException>(() => installation.AppVersion = "1.2.4");
      installation.SetIfDifferent("appVersion", "1.2.4");
      Assert.AreEqual("1.2.4", installation.AppVersion);
    }

    [Test]
    public void TestAppIdentifierGetterSetter() {
      IObjectState state = new MutableObjectState {
        ServerData = new Dictionary<string, object>() {
          { "appIdentifier", "com.parse.app" }
        }
      };
      AVInstallation installation = AVObject.FromState<AVInstallation>(state, "_Installation");
      Assert.NotNull(installation);
      Assert.AreEqual("com.parse.app", installation.AppIdentifier);

      Assert.Throws<InvalidOperationException>(() => installation.AppIdentifier = "com.parse.newapp");
      installation.SetIfDifferent("appIdentifier", "com.parse.newapp");
      Assert.AreEqual("com.parse.newapp", installation.AppIdentifier);
    }

    [Test]
    public void TestTimeZoneGetter() {
      IObjectState state = new MutableObjectState {
        ServerData = new Dictionary<string, object>() {
          { "timeZone", "America/Los_Angeles" }
        }
      };
      AVInstallation installation = AVObject.FromState<AVInstallation>(state, "_Installation");
      Assert.NotNull(installation);
      Assert.AreEqual("America/Los_Angeles", installation.TimeZone);
    }

    [Test]
    public void TestLocaleIdentifierGetter() {
      IObjectState state = new MutableObjectState {
        ServerData = new Dictionary<string, object>() {
          { "localeIdentifier", "en-US" }
        }
      };
      AVInstallation installation = AVObject.FromState<AVInstallation>(state, "_Installation");
      Assert.NotNull(installation);
      Assert.AreEqual("en-US", installation.LocaleIdentifier);
    }

    [Test]
    public void TestChannelGetterSetter() {
      var channels = new List<string>() { "the", "richard" };
      IObjectState state = new MutableObjectState {
        ServerData = new Dictionary<string, object>() {
          { "channels", channels }
        }
      };
      AVInstallation installation = AVObject.FromState<AVInstallation>(state, "_Installation");
      Assert.NotNull(installation);
      Assert.AreEqual("the", installation.Channels[0]);
      Assert.AreEqual("richard", installation.Channels[1]);

      installation.Channels = new List<string>() { "mr", "kevin" };
      Assert.AreEqual("mr", installation.Channels[0]);
      Assert.AreEqual("kevin", installation.Channels[1]);
    }

    [Test]
    public void TestGetCurrentInstallation() {
      var guid = Guid.NewGuid();
      IObjectState state = new MutableObjectState {
        ServerData = new Dictionary<string, object>() {
          { "installationId", guid.ToString() }
        }
      };
      AVInstallation installation = AVObject.FromState<AVInstallation>(state, "_Installation");
      var mockController = new Mock<IAVCurrentInstallationController>();
      mockController.Setup(obj => obj.GetAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(installation));

      AVCorePlugins.Instance.CurrentInstallationController = mockController.Object;

      var currentInstallation = AVInstallation.CurrentInstallation;
      Assert.NotNull(currentInstallation);
      Assert.AreEqual(guid, currentInstallation.InstallationId);
    }
  }
}
