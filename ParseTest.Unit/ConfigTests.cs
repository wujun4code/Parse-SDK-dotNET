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
	public class ConfigTests {
		private IAVConfigController MockedConfigController {
			get {
				var mockedConfigController = new Mock<IAVConfigController>();
				var mockedCurrentConfigController = new Mock<IAVCurrentConfigController>();

				AVConfig theConfig = new AVConfig(new Dictionary<string, object> {{
					"params", new Dictionary<string, object> {{
						 "testKey", "testValue"
					}}
				}});

				mockedCurrentConfigController.Setup(
					obj => obj.GetCurrentConfigAsync()
				).Returns(Task.FromResult(theConfig));

				mockedConfigController.Setup(obj => obj.CurrentConfigController)
            .Returns(mockedCurrentConfigController.Object);

        var tcs = new TaskCompletionSource<AVConfig>();
        tcs.TrySetCanceled();

        mockedConfigController.Setup(obj => obj.FetchConfigAsync(It.IsAny<string>(),
            It.Is<CancellationToken>(ct => ct.IsCancellationRequested))).Returns(tcs.Task);

				mockedConfigController.Setup(obj => obj.FetchConfigAsync(It.IsAny<string>(),
            It.Is<CancellationToken>(ct => !ct.IsCancellationRequested))).Returns(Task.FromResult(theConfig));

				return mockedConfigController.Object;
			}
		}

		[SetUp]
		public void SetUp() {
			AVCorePlugins.Instance.ConfigController = MockedConfigController;
      AVCorePlugins.Instance.CurrentUserController = new Mock<IAVCurrentUserController>().Object;
		}

		[TearDown]
		public void TearDown() {
			AVCorePlugins.Instance.ConfigController = null;
      AVCorePlugins.Instance.CurrentUserController = null;
		}

		[Test]
		public void TestCurrentConfig() {
			AVConfig config = AVConfig.CurrentConfig;

			Assert.AreEqual("testValue", config["testKey"]);
			Assert.AreEqual("testValue", config.Get<string>("testKey"));
		}

		[Test]
		public void TestToJSON() {
			AVConfig config1 = AVConfig.CurrentConfig;
			IDictionary<string, object> expectedJson = new Dictionary<string, object> {{
				"params", new Dictionary<string, object> {{
					"testKey", "testValue"
				}}
			}};

			Assert.AreEqual(((IJsonConvertible)config1).ToJSON(), expectedJson);
		}

		[Test]
		[AsyncStateMachine(typeof(ConfigTests))]
		public Task TestGetConfig() {
			return AVConfig.GetAsync().ContinueWith(t => {
				Assert.AreEqual("testValue", t.Result["testKey"]);
				Assert.AreEqual("testValue", t.Result.Get<string>("testKey"));
			});
		}

		[Test]
		[AsyncStateMachine(typeof(ConfigTests))]
		public Task TestGetConfigCancel() {
			CancellationTokenSource tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();
			return AVConfig.GetAsync(tokenSource.Token).ContinueWith(t => {
				Assert.True(t.IsCanceled);
			});
		}
	}
}
