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
using LeanCloud.Realtime;

namespace ParseTest
{
    [TestFixture]
    public class ConversationTest
    {

        [SetUp]
        public void SetUp()
        {
            Websockets.Net.WebsocketConnection.Link();
        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        [AsyncStateMachine(typeof(ConversationTest))]
        public Task TestConnectAsync()
        {
            var realtime = new AVRealtime("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");
            return realtime.CreateClient("junwu").ContinueWith(t =>
            {
                var client = t.Result;
                Console.WriteLine(realtime.State.ToString());
                return client;
            }).ContinueWith(s =>
            {
                var client = s.Result;
                var admins = new string[] { "zman", "junwu" };
                return client.CreateConversationAsync("abc", false, new Dictionary<string, object>
                {
                    { "admins",admins }
                });
            }).Unwrap().ContinueWith(c =>
            {
                var conv = c.Result;
                Console.WriteLine(conv.ConversationId);
                Assert.NotNull(conv.ConversationId);
                foreach (var key in conv.Keys)
                {
                    Console.WriteLine(conv[key]);
                }
                return Task.FromResult(0);
            });
        }
    }
}
