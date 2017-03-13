using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using LeanCloud;
using LeanCloud.Core.Internal;
using System.Configuration;
using LeanCloud.Storage;

namespace ParseTest
{
    [TestFixture]
    public class QueryTests
    {
        [SetUp]
        public void SetUp()
        {
            string appId = ConfigurationManager.AppSettings["appId"];
            string appKey = ConfigurationManager.AppSettings["appKey"];
            AVClient.Initialize(appId, appKey);
        }

        [Test]
        public Task CQLQueryTest()
        {
            string cql = "select * from Todo where location='会议室'";

            return AVQuery<AVObject>.DoCloudQueryAsync(cql).ContinueWith(t =>
            {
                Assert.False(t.IsFaulted);
                Assert.False(t.IsCanceled);
                Assert.True(t.Result.Count() > 0);
                return Task.FromResult(0);
            });
        }
        [Test]
        public Task CQLQueryWithPlaceholderTest()
        {
            string cql = "select * from Todo where location=?";


            return AVQuery<AVObject>.DoCloudQueryAsync(cql, "会议室").ContinueWith(t =>
             {
                 Assert.False(t.IsFaulted);
                 Assert.False(t.IsCanceled);
                 Assert.True(t.Result.Count() > 0);
                 return Task.FromResult(0);
             });
        }

        [Test]
        public Task CQLQueryWithMultiPlaceholderTest()
        {
            string cql = "select * from Todo where location=? and title=?";


            return AVQuery<AVObject>.DoCloudQueryAsync(cql, "会议室", "发布 SDK").ContinueWith(t =>
            {
                Assert.False(t.IsFaulted);
                Assert.False(t.IsCanceled);
                var todos = t.Result;

                Assert.True(todos.Count() > 0);
                return Task.FromResult(0);
            });
        }
        [Test]
        public Task QueryIncludePonters()
        {
            string cql = "select include folder, * from Todo where title=?";


            return AVQuery<AVObject>.DoCloudQueryAsync(cql, "test todo").ContinueWith(t =>
            {
                Assert.False(t.IsFaulted);
                Assert.False(t.IsCanceled);
                var todos = t.Result;
                var firstTodo = todos.First();
                var folder = firstTodo.Get<AVObject>("folder");

                Assert.IsNotNull(folder.Get<string>("name"));
                return Task.FromResult(0);
            });
        }
        [Test]
        public Task RelationReverseQueryTest()
        {
            var hangzhou = new AVObject("City");
            hangzhou["name"] = "杭州";

            var wenzhou = new AVObject("City");
            wenzhou["name"] = "温州";

            var zhejiang = new AVObject("Province");
            zhejiang.Set("name", "浙江");
            return AVObject.SaveAllAsync(new AVObject[] { hangzhou, wenzhou }).ContinueWith(t =>
             {
                 var relation = zhejiang.GetRelation<AVObject>("includedCities");
                 relation.Add(hangzhou);
                 relation.Add(wenzhou);

                 return zhejiang.SaveAsync();
             }).Unwrap().ContinueWith(s =>
             {
                 var reverseQuery = hangzhou.GetRelationRevserseQuery<AVObject>("Province", "includedCities");
                 return reverseQuery.FindAsync();
             }).Unwrap().ContinueWith(x =>
             {
                 var provinces = x.Result;
                 Assert.IsTrue(provinces.Count() == 1);
                 return Task.FromResult(0);
             });
        }
        [Test]
        public Task QueryWhereSizeEqualTo()
        {
            AVObject obj = new AVObject("TestQueryWhereSizeEqualTo");
            obj["a"] = new List<int>(new int[] { 1, 2, 3});
            return obj.SaveAsync().ContinueWith(_ =>
            {
                var query = new AVQuery<AVObject>("TestQueryWhereSizeEqualTo");
                query.WhereSizeEqualTo("a", 3);
                return query.FindAsync();
            }).Unwrap().ContinueWith(t =>
            {
                var queriedObjects = t.Result;
                Assert.AreEqual(queriedObjects.Count(), 1);
                Assert.AreEqual(queriedObjects.First().ObjectId, obj.ObjectId);
                return obj.DeleteAsync();
            });
        }
    }
}
