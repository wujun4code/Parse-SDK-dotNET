using NUnit.Framework;
using LeanCloud;
using LeanCloud.Core.Internal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using System.Globalization;
using System.Diagnostics;

namespace ParseTest
{
    [TestFixture]
    public class DecoderTests
    {
        [SetUp]
        public void initApp()
        {
            string appId = ConfigurationManager.AppSettings["appId"];
            string appKey = ConfigurationManager.AppSettings["appKey"];
            AVClient.Initialize(appId, appKey);
        }
        [Test]
        public void TestParseDate()
        {
            DateTime dateTime = (DateTime)AVDecoder.Instance.Decode(AVDecoder.ParseDate("1990-08-30T12:03:59.000Z"));
            dateTime = dateTime.ToUniversalTime();
            Assert.AreEqual(1990, dateTime.Year);
            Assert.AreEqual(8, dateTime.Month);
            Assert.AreEqual(30, dateTime.Day);
            Assert.AreEqual(12, dateTime.Hour);
            Assert.AreEqual(3, dateTime.Minute);
            Assert.AreEqual(59, dateTime.Second);
            Assert.AreEqual(0, dateTime.Millisecond);
        }

        [Test]
        public void TestDecodePrimitives()
        {
            Assert.AreEqual(1, AVDecoder.Instance.Decode(1));
            Assert.AreEqual(0.3, AVDecoder.Instance.Decode(0.3));
            Assert.AreEqual("halyosy", AVDecoder.Instance.Decode("halyosy"));

            Assert.IsNull(AVDecoder.Instance.Decode(null));
        }

        [Test]
        public void TestDecodeFieldOperation()
        {
            IDictionary<string, object> value = new Dictionary<string, object>() {
        { "__op", "Increment" },
        { "amount", "322" }
      };

            // Decoding ParseFieldOperation is not supported on .NET now. We only need this for LDS.
            Assert.Throws<NotImplementedException>(() => AVDecoder.Instance.Decode(value));
        }

        [Test]
        public void TestDecodeDate()
        {
            IDictionary<string, object> value = new Dictionary<string, object>() {
        { "__type", "Date" },
        { "iso", "1990-08-30T12:03:59.000Z" }
      };

            DateTime dateTime = (DateTime)AVDecoder.Instance.Decode(value);
            dateTime = dateTime.ToUniversalTime();
            Assert.AreEqual(1990, dateTime.Year);
            Assert.AreEqual(8, dateTime.Month);
            Assert.AreEqual(30, dateTime.Day);
            Assert.AreEqual(12, dateTime.Hour);
            Assert.AreEqual(3, dateTime.Minute);
            Assert.AreEqual(59, dateTime.Second);
            Assert.AreEqual(0, dateTime.Millisecond);
        }

        [Test]
        public void TestDecodeImproperDate()
        {
            IDictionary<string, object> value = new Dictionary<string, object>() {
        { "__type", "Date" },
        { "iso", "1990-08-30T12:03:59.0Z" }
      };

            DateTime dateTime = (DateTime)AVDecoder.Instance.Decode(value);
            dateTime = dateTime.ToUniversalTime();
            Assert.AreEqual(1990, dateTime.Year);
            Assert.AreEqual(8, dateTime.Month);
            Assert.AreEqual(30, dateTime.Day);
            Assert.AreEqual(12, dateTime.Hour);
            Assert.AreEqual(3, dateTime.Minute);
            Assert.AreEqual(59, dateTime.Second);
            Assert.AreEqual(0, dateTime.Millisecond);

            // Test multiple trailing zeroes
            value = new Dictionary<string, object>() {
        { "__type", "Date" },
        { "iso", "1990-08-30T12:03:59.00Z" }
      };

            dateTime = (DateTime)AVDecoder.Instance.Decode(value);
            dateTime = dateTime.ToUniversalTime();
            Assert.AreEqual(1990, dateTime.Year);
            Assert.AreEqual(8, dateTime.Month);
            Assert.AreEqual(30, dateTime.Day);
            Assert.AreEqual(12, dateTime.Hour);
            Assert.AreEqual(3, dateTime.Minute);
            Assert.AreEqual(59, dateTime.Second);
            Assert.AreEqual(0, dateTime.Millisecond);
        }

        [Test]
        public void TestDecodeBytes()
        {
            IDictionary<string, object> value = new Dictionary<string, object>() {
        { "__type", "Bytes" },
        { "base64", "VGhpcyBpcyBhbiBlbmNvZGVkIHN0cmluZw==" }
      };

            byte[] bytes = AVDecoder.Instance.Decode(value) as byte[];
            Assert.AreEqual("This is an encoded string", System.Text.Encoding.UTF8.GetString(bytes));
        }

        [Test]
        public void TestDecodePointer()
        {
            IDictionary<string, object> value = new Dictionary<string, object>() {
        { "__type", "Pointer" },
        { "className", "Corgi" },
        { "objectId", "lLaKcolnu" }
      };

            AVObject obj = AVDecoder.Instance.Decode(value) as AVObject;
            Assert.IsFalse(obj.IsDataAvailable);
            Assert.AreEqual("Corgi", obj.ClassName);
            Assert.AreEqual("lLaKcolnu", obj.ObjectId);
        }

        [Test]
        public void TestDecodeFile()
        {
            IDictionary<string, object> value1 = new Dictionary<string, object>() {
        { "__type", "File" },
        { "name", "Corgi.png" },
        { "url", "http://corgi.xyz/gogo.png" }
      };

            AVFile file1 = AVDecoder.Instance.Decode(value1) as AVFile;
            Assert.AreEqual("Corgi.png", file1.Name);
            Assert.AreEqual("http://corgi.xyz/gogo.png", file1.Url.AbsoluteUri);
            Assert.IsFalse(file1.IsDirty);

            IDictionary<string, object> value2 = new Dictionary<string, object>() {
        { "__type", "File" },
        { "name", "Corgi.png" }
      };

            Assert.Throws<KeyNotFoundException>(() => AVDecoder.Instance.Decode(value2));
        }

        [Test]
        public void TestDecodeGeoPoint()
        {
            IDictionary<string, object> value1 = new Dictionary<string, object>() {
        { "__type", "GeoPoint" },
        { "latitude", 0.9 },
        { "longitude", 0.3 }
      };
            AVGeoPoint point1 = (AVGeoPoint)AVDecoder.Instance.Decode(value1);
            Assert.IsNotNull(point1);
            Assert.AreEqual(0.9, point1.Latitude);
            Assert.AreEqual(0.3, point1.Longitude);

            IDictionary<string, object> value2 = new Dictionary<string, object>() {
        { "__type", "GeoPoint" },
        { "latitude", 0.9 }
      };
            Assert.Throws<KeyNotFoundException>(() => AVDecoder.Instance.Decode(value2));
        }

        [Test]
        public void TestDecodeObject()
        {
            IDictionary<string, object> value = new Dictionary<string, object>() {
        { "__type", "Object" },
        { "className", "Corgi" },
        { "objectId", "lLaKcolnu" },
        { "createdAt", "2015-06-22T21:23:41.733Z" },
        { "updatedAt", "2015-06-22T22:06:41.733Z" }
      };

            AVObject obj = AVDecoder.Instance.Decode(value) as AVObject;
            Assert.IsTrue(obj.IsDataAvailable);
            Assert.AreEqual("Corgi", obj.ClassName);
            Assert.AreEqual("lLaKcolnu", obj.ObjectId);
            Assert.IsNotNull(obj.CreatedAt);
            Assert.IsNotNull(obj.UpdatedAt);
        }

        [Test]
        public void TestDecodeRelation()
        {
            IDictionary<string, object> value = new Dictionary<string, object>() {
        { "__type", "Relation" },
        { "className", "Corgi" },
        { "objectId", "lLaKcolnu" }
      };

            AVRelation<AVObject> relation = AVDecoder.Instance.Decode(value) as AVRelation<AVObject>;
            Assert.IsNotNull(relation);
            Assert.AreEqual("Corgi", relation.GetTargetClassName());
        }

        [Test]
        public void TestDecodeDictionary()
        {
            IDictionary<string, object> value = new Dictionary<string, object>() {
        { "megurine", "luka" },
        { "hatsune", new AVObject("Miku") },
        {
          "decodedGeoPoint", new Dictionary<string, object>() {
            { "__type", "GeoPoint" },
            { "latitude", 0.9 },
            { "longitude", 0.3 }
          }
        },
        {
          "listWithSomething", new List<object>() {
            new Dictionary<string, object>() {
              { "__type", "GeoPoint" },
              { "latitude", 0.9 },
              { "longitude", 0.3 }
            }
          }
        }
      };

            IDictionary<string, object> dict = AVDecoder.Instance.Decode(value) as IDictionary<string, object>;
            Assert.AreEqual("luka", dict["megurine"]);
            Assert.IsTrue(dict["hatsune"] is AVObject);
            Assert.IsTrue(dict["decodedGeoPoint"] is AVGeoPoint);
            Assert.IsTrue(dict["listWithSomething"] is IList<object>);
            var decodedList = dict["listWithSomething"] as IList<object>;
            Assert.IsTrue(decodedList[0] is AVGeoPoint);

            IDictionary<object, string> randomValue = new Dictionary<object, string>() {
        { "ultimate", "elements" },
        { new AVACL(), "lLaKcolnu" }
      };

            IDictionary<object, string> randomDict = AVDecoder.Instance.Decode(randomValue) as IDictionary<object, string>;
            Assert.AreEqual("elements", randomDict["ultimate"]);
            Assert.AreEqual(2, randomDict.Keys.Count);
        }

        [Test]
        public void TestDecodeList()
        {
            IList<object> value = new List<object>() {
        1, new AVACL(), "wiz",
        new Dictionary<string, object>() {
          { "__type", "GeoPoint" },
          { "latitude", 0.9 },
          { "longitude", 0.3 }
        }, new List<object>() {
          new Dictionary<string, object>() {
            { "__type", "GeoPoint" },
            { "latitude", 0.9 },
            { "longitude", 0.3 }
          }
        }
      };

            IList<object> list = AVDecoder.Instance.Decode(value) as IList<object>;
            Assert.AreEqual(1, list[0]);
            Assert.IsTrue(list[1] is AVACL);
            Assert.AreEqual("wiz", list[2]);
            Assert.IsTrue(list[3] is AVGeoPoint);
            Assert.IsTrue(list[4] is IList<object>);
            var decodedList = list[4] as IList<object>;
            Assert.IsTrue(decodedList[0] is AVGeoPoint);
        }

        [Test]
        public void TestDecodeArray()
        {
            int[] value = new int[] { 1, 2, 3, 4 };

            int[] array = AVDecoder.Instance.Decode(value) as int[];
            Assert.AreEqual(4, array.Length);
            Assert.AreEqual(1, array[0]);
            Assert.AreEqual(2, array[1]);
        }
        [Test]
        public Task TestDecodeStringList()
        {
            var todo = new AVObject("Todo");
            todo["listString"] = new List<string>()
            {
                "str1",
                "str2",
                "str3"
            };
            return todo.SaveAsync().ContinueWith(t =>
            {
                var query = new AVQuery<AVObject>("Todo").WhereEqualTo("objectId", todo.ObjectId);
                return query.FirstAsync();
            }).Unwrap().ContinueWith(s =>
            {
                var listString = s.Result.Get<List<Object>>("listString");
                Assert.True(listString.Count == 3);
            });
        }

        [Test]
        public Task TestDecodeDateTimeTimeZone()
        {
            var todo = new AVObject("Todo");
            var datetimeStr = "2017-04-02";
            AVClient.HttpLog(s => { Debug.WriteLine(s); });
            //var testDateTimeObj = todo["testDateTime"] = DateTime.ParseExact(datetimeStr, "yyyy-MM-dd", null, DateTimeStyles.AssumeLocal);
            var testDateTimeObj = DateTime.Now;
            todo["testDateTime"] = testDateTimeObj;
            return todo.SaveAsync().ContinueWith(t =>
            {
                var query = new AVQuery<AVObject>("Todo").WhereEqualTo("objectId", todo.ObjectId);
                return query.FirstAsync();
            }).Unwrap().ContinueWith(s =>
            {
                var afterSavedTestDateTimeObj = s.Result.Get<DateTime>("testDateTime");
                Assert.True(afterSavedTestDateTimeObj.Hour.Equals(testDateTimeObj.Hour));
            });
        }
    }
}
