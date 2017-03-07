using System;
using NUnit.Framework;
using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LeanCloud.Realtime.Test.Unit.NetFx45
{
    [TestFixture]
    public class UnitTest1
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
    }
}
