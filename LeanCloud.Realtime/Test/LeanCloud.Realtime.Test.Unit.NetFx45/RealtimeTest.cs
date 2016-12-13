using Moq;
using NUnit.Framework;
using LeanCloud;
using LeanCloud.Core.Internal;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;


namespace LeanCloud.Realtime.Test.Unit.NetFx45
{
    [TestFixture]
    public class RealtimeTest
    {
        [SetUp]
        public void SetUp()
        {

        }

        [TearDown]
        public void TearDown()
        {
            AVPlugins.Instance.Reset();
        }
    }
}
