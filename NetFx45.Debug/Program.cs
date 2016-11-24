using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeanCloud;

namespace NetFx45.Debug
{
    class Program
    {
        static void Main(string[] args)
        {


            AVClient.Initialize("3knLr8wGGKUBiXpVAwDnryNT-gzGzoHsz", "3RpBhjoPXJjVWvPnVmPyFExt");
            string filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "1.png");
            AVClient.EnableDebugLog(Console.WriteLine, true, true);
            try
            {
                var fileList = new List<AVFile>();
                for (int i = 0; i < 20; i++)
                {
                    AVFile file = CreateFileWithLocalPath(Path.GetFileName(filePath), filePath);
                    fileList.Add(file);
                }
                var done = 0;
                fileList.ForEach(x =>
                {
                    x.SaveAsync().Wait();
                    done++;
                    Console.WriteLine(done + ".done");
                });
                //AVFile file = CreateFileWithLocalPath(Path.GetFileName(filePath), filePath);

                //var task = file.SaveAsync();
                //task.Wait();
                //AVFile.GetFileWithObjectIdAsync("5836898c67f3560065f3038d").ContinueWith(t =>
                //{
                //    Console.WriteLine(t.Result.ObjectId);
                //}).Wait();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }

            Console.ReadKey();
        }

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
