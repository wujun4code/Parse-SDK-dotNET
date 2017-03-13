using LeanCloud;
using LeanCloud.Core.Internal;
using NUnit.Framework;
using Moq;
using System;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ParseTest
{
    [TestFixture]
    public class UserControllerTests
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

    }
}
