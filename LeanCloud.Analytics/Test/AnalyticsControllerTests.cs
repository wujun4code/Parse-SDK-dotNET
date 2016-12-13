using Moq;
using NUnit.Framework;
using LeanCloud;
using LeanCloud.Analytics.Internal;
using LeanCloud.Core.Internal;
using LeanCloud.Storage.Internal;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using AssemblyLister;

namespace ParseTest
{
    [TestFixture]
    public class AnalyticsControllerTests
    {
        [SetUp]
        public void SetUp()
        {
            AVClient.Initialize(new AVClient.Configuration
            {
                ApplicationId = "",
                ApplicationKey = ""
            });
        }


        private Mock<IAVCommandRunner> CreateMockRunner(Tuple<HttpStatusCode, IDictionary<string, object>> response)
        {
            var mockRunner = new Mock<IAVCommandRunner>();
            mockRunner.Setup(obj => obj.RunCommandAsync(It.IsAny<AVCommand>(),
                It.IsAny<IProgress<AVUploadProgressEventArgs>>(),
                It.IsAny<IProgress<AVDownloadProgressEventArgs>>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task<Tuple<HttpStatusCode, IDictionary<string, object>>>.FromResult(response));

            return mockRunner;
        }
    }
}
