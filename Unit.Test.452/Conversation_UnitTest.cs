using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeanCloud;
using LeanMessage;

namespace Unit.Test._452
{
    /// <summary>
    /// Summary description for Conversation_UnitTest
    /// </summary>
    [TestClass]
    public class Conversation_UnitTest
    {
        public Conversation_UnitTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Create_Conversation()
        {
            AVClient.Initialize("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");
            var client = new AVIMClient("Tom");

            AVIMConversation convseation = new AVIMConversation()
            {
                Name = "xman",
                IsTransient = false,
            };
            convseation.MemberIds = new List<string>();
            convseation.MemberIds.Add("Jerry");

            client.ConnectAsync().ContinueWith(_ =>
            {
                client.CreateConversationAsync(convseation, true).Wait();
            }).Wait();
        }

        [TestMethod]
        public void Conversation_Send_Text_Message()
        {
            AVClient.Initialize("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");
            var client = new AVIMClient("Tom");

            AVIMConversation conversation = AVIMConversation.CreateWithoutData("573df12679df540060417452", client);

            client.ConnectAsync().ContinueWith(_ =>
            {
                var text = new AVIMTextMessage("Hi,Jerry");
                conversation.SendMessageAsync<AVIMTextMessage>(text).Wait();
            }).Wait();
        }
    }
}
