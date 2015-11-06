using Moq;
using NUnit.Framework;
using LeanCloud;
using LeanCloud.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloudTest {
  [TestFixture]
  public class FileTests {
    [TearDown]
    public void TearDown() {
      AVCorePlugins.Instance.FileController = null;
      AVCorePlugins.Instance.CurrentUserController = null;
    }

    [Test]
    [AsyncStateMachine(typeof(FileTests))]
    public Task TestFileSave() {
      var response = new FileState {
        Name = "newBekti.png",
        Url = new Uri("https://www.api.leancloud.cn/newBekti.png"),
        MimeType = "image/png"
      };
      var mockController = new Mock<IAVFileController>();
      mockController.Setup(obj => obj.SaveAsync(It.IsAny<FileState>(),
          It.IsAny<Stream>(),
          It.IsAny<string>(),
          It.IsAny<IProgress<AVUploadProgressEventArgs>>(),
          It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));
      var mockCurrentUserController = new Mock<IAVCurrentUserController>();
      AVCorePlugins.Instance.FileController = mockController.Object;
      AVCorePlugins.Instance.CurrentUserController = mockCurrentUserController.Object;

      AVFile file = new AVFile("bekti.jpeg", new MemoryStream(), "image/jpeg");
      Assert.AreEqual("bekti.jpeg", file.Name);
      Assert.AreEqual("image/jpeg", file.MimeType);
      Assert.True(file.IsDirty);

      return file.SaveAsync().ContinueWith(t => {
        Assert.False(t.IsFaulted);
        Assert.AreEqual("newBekti.png", file.Name);
        Assert.AreEqual("image/png", file.MimeType);
        Assert.AreEqual("https://www.api.leancloud.cn/newBekti.png", file.Url.AbsoluteUri);
        Assert.False(file.IsDirty);
      });
    }
  }
}
