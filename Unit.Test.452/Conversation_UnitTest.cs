using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeanCloud;
using LeanMessage;
using System.Diagnostics;
using System.IO;

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
            client.UseLeanEngineSignatureFactory();
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
            AVClient.Initialize("JXyR8vfpeSr8cfaYnob2zYl0-9Nh9j0Va", "Fgq2YlPdnP1KJEoWyF5tk2az");
            var client = new AVIMClient("Tom");

            AVIMConversation conversation = AVIMConversation.CreateWithoutData("573df12679df540060417452", client);

            client.ConnectAsync().ContinueWith(_ =>
            {
                client.RegisterMessage<AVIMTextMessage>((message) =>
                {
                    var textMessage = message as AVIMTextMessage;
                    Trace.WriteLine(textMessage.TextContent);
                });

                client.RegisterMessage<AVIMAudioMessage>((audio) =>
                {

                });

                var text = new AVIMTextMessage("Hi,Jerry");
                conversation.SendMessageAsync(text).ContinueWith(t =>
                {
                    Trace.WriteLine(text.Id);
                }).Wait();
            }).Wait();
        }

        [TestMethod]
        public void Conversation_Send_Image_Message()
        {
            AVClient.Initialize("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");
            var client = new AVIMClient("Tom");

            client.ConnectAsync().ContinueWith(_ =>
            {
                AVIMImageMessage message = AVIMImageMessage.FromUrl("http://ww3.sinaimg.cn/bmiddle/596b0666gw1ed70eavm5tg20bq06m7wi.gif");
            }).Wait();
        }

        [TestMethod]
        public void AVFile_Image_From_Url()
        {
            AVClient.Initialize("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");
            AVFile avfile = new AVFile("yoshi.jpg", "http://ww3.sinaimg.cn/bmiddle/596b0666gw1ed70eavm5tg20bq06m7wi.gif");
            avfile.SaveAsync().Wait();
        }

        [TestMethod]
        public void AVFile_File_From_String()
        {
            AVClient.Initialize("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");
            AVClient.EnableDebugLog((obj) =>
            {
                Trace.WriteLine(obj);
            });
            byte[] data = System.Text.Encoding.UTF8.GetBytes("LeanCloud is great good wonderful great good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderfulgreat good wonderful!");
            AVFile file = new AVFile("resume.txt", data);
            file.SaveAsync().Wait();
        }

        [TestMethod]
        public void AVFile_File_From_String_QCloud()
        {
            AVClient.Initialize("JXyR8vfpeSr8cfaYnob2zYl0-9Nh9j0Va", "Fgq2YlPdnP1KJEoWyF5tk2az");
            AVClient.EnableDebugLog((obj) =>
            {
                Trace.WriteLine(obj);
            });
            byte[] data = System.Text.Encoding.UTF8.GetBytes("LeanCloud is great!");
            AVFile file = new AVFile("resume.txt", data);
            file.SaveAsync().Wait();
        }

        [TestMethod]
        public void AVFile_File_From_File_QCloud()
        {
            AVClient.Initialize("JXyR8vfpeSr8cfaYnob2zYl0-9Nh9j0Va", "Fgq2YlPdnP1KJEoWyF5tk2az");
            AVClient.EnableDebugLog((obj) =>
            {
                Trace.WriteLine(obj);
            });
            var ss = Directory.GetCurrentDirectory();
            var path = Path.Combine(ss, "Jay.mp3");
            var fileStream = new FileStream(path, FileMode.Open);

                // read from file or write to file
            AVFile file = new AVFile("Jay.mp3", fileStream);
            file.SaveAsync().Wait();
        }

        [TestMethod]
        public void Conversation_Join()
        {
            AVClient.Initialize("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");
            var client = new AVIMClient("John");
            client.UseLeanEngineSignatureFactory();

            client.ConnectAsync().ContinueWith(_ =>
            {
                AVIMConversation consersation = AVIMConversation.CreateWithoutData("575e88521532bc0060995d32", client);
                consersation.JoinAsync().Wait();
            }).Wait();
        }
    }
}
