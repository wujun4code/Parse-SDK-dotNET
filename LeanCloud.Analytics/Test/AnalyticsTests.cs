using Moq;
using NUnit.Framework;
using LeanCloud;
using LeanCloud.Analytics.Internal;
using LeanCloud.Core.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ParseTest
{
    [TestFixture]
    public class AnalyticsTests
    {
        [TearDown]
        public void TearDown()
        {
            AVAnalyticsPlugins.Instance.Reset();
        }
    }
}
