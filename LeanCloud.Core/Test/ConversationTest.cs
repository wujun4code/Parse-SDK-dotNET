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
        AVRealtime avRealtime;
        [Test]
        public async void TestConnectAsync()
        {
            var realtime = new AVRealtime("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");
            var client = await realtime.CreateClient("junwu");
            var admins = new string[] { "zman", "junwu" };
            var conv = await client.CreateConversationAsync("abc", false, new Dictionary<string, object>
                {
                    { "admins",admins }
                });
        }

        //[Test]
        //[AsyncStateMachine(typeof(ConversationTest))]
        //public Task TestTextMessageReceieved()
        //{
        //    var realtime = new AVRealtime("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");
        //    return realtime.CreateClient("junwu").ContinueWith(t =>
        //    {
        //        var client = t.Result;
        //        Console.WriteLine(realtime.State.ToString());

        //        var tcs = new TaskCompletionSource<AVIMTextMessageEventArgs>();

        //        var textMessageListener = new AVIMTextMessageListener();
        //        client.RegisterListener(textMessageListener);
        //        textMessageListener.OnTextMessageReceieved += (sender, args) =>
        //        {
        //            Console.WriteLine(args.TextMessage.TextContent);
        //            tcs.SetResult(args);
        //        };

        //        return tcs.Task;
        //    });
        //}

        [Test]
        [AsyncStateMachine(typeof(ConversationTest))]
        public Task TestConversationQueryFindAsync()
        {
            var realtime = new AVRealtime("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");

            return realtime.CreateClient("junwu").ContinueWith(t =>
            {
                var client = t.Result;
                var query = client.GetQuery();
                return query.FindAsync();
            }).Unwrap().ContinueWith(s =>
            {
                var cons = s.Result;
                foreach (var con in cons)
                {
                    Console.WriteLine(con.Name);
                }
                Assert.True(cons.Count() > 0);
                return Task.FromResult(0);
            });
        }

        [Test]
        [AsyncStateMachine(typeof(ConversationTest))]
        public Task TestConversationQueryFirstAsync()
        {
            var realtime = new AVRealtime("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");

            return realtime.CreateClient("junwu").ContinueWith(t =>
            {
                var client = t.Result;
                var query = client.GetQuery();
                return query.FirstAsync();
            }).Unwrap().ContinueWith(s =>
            {
                var con = s.Result;

                Assert.NotNull(con.ConversationId);
                return Task.FromResult(0);
            });
        }

    }
}
