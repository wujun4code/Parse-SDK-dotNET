using System;
using NUnit.Framework;
using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LeanCloud.Realtime.Test.Unit.NetFx45
{
    [TestFixture]
    public class RealtimeTest
    {
        AVRealtime avRealtime;
        [SetUp]
        public void initApp()
        {
            Websockets.Net.WebsocketConnection.Link();
            string appId = ConfigurationManager.AppSettings["appId"];
            string appKey = ConfigurationManager.AppSettings["appKey"];
            avRealtime = new AVRealtime(appId, appKey);

            AVClient.HttpLog(Console.WriteLine);
        }

        [Test]
        public async Task TestConversationQueryFindAsync()
        {

            var client = await avRealtime.CreateClient("junwu");
            var query = client.GetQuery();
            var con = await query.FirstAsync();
            Console.WriteLine(con.CreatedAt);

            await Task.FromResult(0);
        }

        [Test]
        public async Task CreateConversation()
        {
            var client = await avRealtime.CreateClient("junwu");
            var conversation = await client.CreateConversationAsync("wchen",
                options: new Dictionary<string, object>()
                {
                    { "type","private"}
                });
        }
        [Test]
        public async Task SendTextMessage()
        {
            var client = await avRealtime.CreateClient("junwu");
            var conversation = await client.CreateConversationAsync("wchen",
                options: new Dictionary<string, object>()
                {
                    { "type","private"}
                });
            AVIMTextMessage textMessage = new AVIMTextMessage("fuck mono");
            await conversation.SendMessageAsync(textMessage);
        }

        [Test]
        public async Task TestTimeZone()
        {
            var list = await new AVQuery<AVObject>("TestObject").FindAsync();
            foreach (var item in list)
            {
                Console.WriteLine(item.CreatedAt);
            }
        }
        [Test]
        public async Task AccessDictionary()
        {
            var todo = new AVObject("Todo");
            var metaData = new Dictionary<string, object>();
            metaData.Add("a", 1);
            metaData.Add("b", false);
            metaData.Add("c", "value");
            todo.Add("metaData", metaData);
            await todo.SaveAsync();
            var query = new AVQuery<AVObject>("Todo");
            query = query.WhereEqualTo("objectId", todo.ObjectId);
            var result = await query.FindAsync();
            foreach (var t in result)
            {
                var accesssMetaData = t.Get<Dictionary<string, object>>("metaData");
                Console.WriteLine(accesssMetaData["a"]);
            }
            await Task.Delay(0);
        }
    }
}
