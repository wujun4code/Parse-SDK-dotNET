using LeanCloud;
using LeanCloud.Internal;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace LeanCloudTest {
  [TestFixture]
  public class CurrentInstallationControllerTests {
    [SetUp]
    public void SetUp() {
      AVObject.RegisterSubclass<AVInstallation>();
    }

    [TearDown]
    public void TearDown() {
      AVObject.UnregisterSubclass(AVObject.GetClassName(typeof(AVInstallation)));
    }

    [Test]
    public void TestConstructor() {
      var mockInstallationIdController = new Mock<IInstallationIdController>();
      var guid = Guid.NewGuid();
      mockInstallationIdController.Setup(obj => obj.Get()).Returns(guid);

      var controller = new AVCurrentInstallationController(new Mock<IInstallationIdController>().Object);
      Assert.IsNull(controller.CurrentInstallation);
    }

    [Test]
    [AsyncStateMachine(typeof(CurrentInstallationControllerTests))]
    public Task TestGetSetAsync() {
      var mockInstallationIdController = new Mock<IInstallationIdController>();
      var guid = Guid.NewGuid();
      mockInstallationIdController.Setup(obj => obj.Get()).Returns(guid);

      var controller = new AVCurrentInstallationController(mockInstallationIdController.Object);
      var installation = new AVInstallation();

      return controller.SetAsync(installation, CancellationToken.None).OnSuccess(_ => {
        Assert.AreEqual(installation, controller.CurrentInstallation);

        return controller.GetAsync(CancellationToken.None);
      }).Unwrap()
      .OnSuccess(t => {
        Assert.AreEqual(installation, controller.CurrentInstallation);

        controller.ClearFromMemory();
        Assert.AreNotEqual(installation, controller.CurrentInstallation);

        return controller.GetAsync(CancellationToken.None);
      }).Unwrap()
      .OnSuccess(t => {
        Assert.AreNotSame(installation, controller.CurrentInstallation);
        Assert.IsNotNull(controller.CurrentInstallation);
      });
    }

    [Test]
    [AsyncStateMachine(typeof(CurrentInstallationControllerTests))]
    public Task TestExistsAsync() {
      var mockInstallationIdController = new Mock<IInstallationIdController>();
      var guid = Guid.NewGuid();
      mockInstallationIdController.Setup(obj => obj.Get()).Returns(guid);

      var controller = new AVCurrentInstallationController(mockInstallationIdController.Object);
      var installation = new AVInstallation();

      return controller.SetAsync(installation, CancellationToken.None).OnSuccess(_ => {
        Assert.AreEqual(installation, controller.CurrentInstallation);
        return controller.ExistsAsync(CancellationToken.None);
      }).Unwrap()
      .OnSuccess(t => {
        Assert.IsTrue(t.Result);

        controller.ClearFromMemory();

        return controller.ExistsAsync(CancellationToken.None);
      }).Unwrap()
      .OnSuccess(t => {
        Assert.IsTrue(t.Result);

        controller.ClearFromDisk();

        return controller.ExistsAsync(CancellationToken.None);
      }).Unwrap()
      .OnSuccess(t => {
        Assert.IsFalse(t.Result);
      });
    }

    [Test]
    [AsyncStateMachine(typeof(CurrentInstallationControllerTests))]
    public Task TestIsCurrent() {
      var mockInstallationIdController = new Mock<IInstallationIdController>();
      var guid = Guid.NewGuid();
      mockInstallationIdController.Setup(obj => obj.Get()).Returns(guid);

      var controller = new AVCurrentInstallationController(mockInstallationIdController.Object);
      var installation = new AVInstallation();
      var installation2 = new AVInstallation();

      return controller.SetAsync(installation, CancellationToken.None).OnSuccess(t => {
        Assert.IsTrue(controller.IsCurrent(installation));
        Assert.IsFalse(controller.IsCurrent(installation2));

        controller.ClearFromMemory();

        Assert.IsFalse(controller.IsCurrent(installation));

        return controller.SetAsync(installation, CancellationToken.None);
      }).Unwrap()
      .OnSuccess(t => {
        Assert.IsTrue(controller.IsCurrent(installation));
        Assert.IsFalse(controller.IsCurrent(installation2));

        controller.ClearFromDisk();

        Assert.IsFalse(controller.IsCurrent(installation));

        return controller.SetAsync(installation2, CancellationToken.None);
      }).Unwrap()
      .OnSuccess(t => {
        Assert.IsFalse(controller.IsCurrent(installation));
        Assert.IsTrue(controller.IsCurrent(installation2));
      });
    }
  }
}
