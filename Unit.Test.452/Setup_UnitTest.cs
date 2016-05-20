using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using LeanCloud;
using System.Threading;
using System.Diagnostics;
using LeanMessage;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class Setup_UnitTest
    {
        [TestMethod]
        public void AVClient_Initialize()
        {
            AVClient.Initialize("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");

            AVObject todo = new AVObject("Todo");
            todo["location"] = "hehe";
            todo["list"] = new List<string>() { "Jerry","Tom"};
            todo.SaveAsync().ContinueWith(t => {
                Assert.IsNotNull(todo.ObjectId);

                Trace.WriteLine(todo.ObjectId);
            }).Wait();
        }
        [TestMethod]
        public void AVIMClient_SyncRouter()
        {
            AVClient.Initialize("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");
            var client = new AVIMClient("Tom");

            AVIMClient.RouterController.GetAsync(CancellationToken.None).ContinueWith(t =>
            {
                Trace.WriteLine(t.Result.server);
            }).Wait();
        }

        [TestMethod]
        public void AVIMClient_Connect()
        {
            AVClient.Initialize("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");
            var client = new AVIMClient("Tom");

            client.ConnectAsync().Wait();
        }
    }
}
