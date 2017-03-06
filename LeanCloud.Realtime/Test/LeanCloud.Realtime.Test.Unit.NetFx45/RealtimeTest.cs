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
            Websockets.Net.WebsocketConnection.Link();
        }

        [TearDown]
        public void TearDown()
        {
            AVPlugins.Instance.Reset();
        }

        AVRealtime avRealtime;
        [Test]
        public async Task TestConnectAsync()
        {
            avRealtime = new AVRealtime("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");
            var client = await avRealtime.CreateClient("junwu");
            var admins = new string[] { "zman", "junwu" };
            var conv = await client.CreateConversationAsync("abc", false, new Dictionary<string, object>
                {
                    { "admins",admins }
                });

            await Task.Delay(0);
        }
    }
}
