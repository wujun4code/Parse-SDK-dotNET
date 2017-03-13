using Moq;
using NUnit.Framework;
using LeanCloud;
using LeanCloud.Core.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace ParseTest
{
    [TestFixture]
    public class FileTests
    {
        [SetUp]
        public void SetUp()
        {
            string appId = ConfigurationManager.AppSettings["appId"];
            string appKey = ConfigurationManager.AppSettings["appKey"];
            AVClient.Initialize(appId, appKey);
        }

        [TearDown]
        public void TearDown()
        {
            AVPlugins.Instance = null;
        }

        // [Test]
        // public void TestFileSave()
        // {
        //     string filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "fileTest.mp3");
        //     var fileList = new List<AVFile>();
        //     for (int i = 0; i < 1; i++)
        //     {
        //         AVFile file = CreateFileWithLocalPath(Path.GetFileName(filePath), filePath);
        //         fileList.Add(file);
        //     }
        //     var done = 0;
        //     fileList.ForEach(x =>
        //     {
        //         x.SaveAsync().Wait();
        //         done++;
        //         Assert.NotNull(x.ObjectId);
        //     });
        // }

        public static AVFile CreateFileWithLocalPath(string name, string path)
        {
            byte[] buffer;
            FileStream fileStream;
            fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);

            try
            {
                int length = (int)fileStream.Length;  // get file length
                buffer = new byte[length];            // create buffer
                int count;                            // actual number of bytes read
                int sum = 0;                          // total number of bytes read

                // read until Read method returns 0 (end of the stream has been reached)
                while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;  // sum is a buffer offset for next reading
            }
            finally
            {
                fileStream.Close();
            }
            return new AVFile(name, buffer);
        }
    }
}
