using LeanCloud;
using LeanCloud.Internal;
using NUnit.Framework;
using System;

namespace LeanCloudTest {
  [TestFixture]
  public class InstallationIdControllerTests {
    [TearDown]
    public void TearDown() {
      AVClient.ApplicationSettings.Clear();
    }

    [Test]
    public void TestConstructor() {
      var controller = new InstallationIdController();
      Assert.False(AVClient.ApplicationSettings.ContainsKey("InstallationId"));
    }

    [Test]
    public void TestGet() {
      var controller = new InstallationIdController();
      var installationId = controller.Get();
      Assert.True(AVClient.ApplicationSettings.ContainsKey("InstallationId"));

      AVClient.ApplicationSettings.Clear();

      var newInstallationId = controller.Get();
      Assert.AreEqual(installationId, newInstallationId);
      Assert.False(AVClient.ApplicationSettings.ContainsKey("InstallationId"));

      controller.Clear();

      newInstallationId = controller.Get();
      Assert.AreNotEqual(installationId, newInstallationId);
      Assert.True(AVClient.ApplicationSettings.ContainsKey("InstallationId"));
    }

    [Test]
    public void TestSet() {
      var controller = new InstallationIdController();
      var installationId = controller.Get();
      Assert.True(AVClient.ApplicationSettings.ContainsKey("InstallationId"));

      var installationId2 = Guid.NewGuid();
      controller.Set(installationId2);
      Assert.True(AVClient.ApplicationSettings.ContainsKey("InstallationId"));
      Assert.AreEqual(installationId2.ToString(), AVClient.ApplicationSettings["InstallationId"]);

      var installationId3 = controller.Get();
      Assert.AreEqual(installationId2, installationId3);

      AVClient.ApplicationSettings.Clear();

      controller.Set(installationId);
      Assert.True(AVClient.ApplicationSettings.ContainsKey("InstallationId"));
      Assert.AreEqual(installationId.ToString(), AVClient.ApplicationSettings["InstallationId"]);

      controller.Clear();

      controller.Set(installationId2);
      Assert.True(AVClient.ApplicationSettings.ContainsKey("InstallationId"));
      Assert.AreEqual(installationId2.ToString(), AVClient.ApplicationSettings["InstallationId"]);
    }
  }
}
