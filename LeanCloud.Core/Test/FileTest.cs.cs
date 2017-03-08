using System;
using NUnit.Framework;
using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using LeanCloud;

namespace ParseTest
{
    [TestFixture]
    public class FileTest
    {
        [SetUp]
        public void initApp()
        {
            string appId = ConfigurationManager.AppSettings["appId"];
            string appKey = ConfigurationManager.AppSettings["appKey"];
            AVClient.Initialize(appId, appKey);
        }

        [Test]
        public async Task TestConversationQueryFindAsync()
        {
            var file = await AVFile.GetFileWithObjectIdAsync("58aa82e32f301e006c33b475");

            await file.DeleteAsync();

            await Task.FromResult(0);
        }
    }
}
