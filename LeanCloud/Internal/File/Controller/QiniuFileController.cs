using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud.Internal {
    enum CommonSize:int {
        MB4 = 1024 * 1024 * 4,
        MB1 = 1024 * 1024,
        KB512 = 1024 * 1024 / 2,
        KB256 = 1024 * 1024 / 4
    }

    internal class QiniuFileController:IAVFileController {
        private static int BLOCKSIZE = 1024 * 1024 / 2;
        private const int blockMashk = ( 1 << blockBits ) - 1;
        private const int blockBits = 22;
        private int CalcBlockCount(long fsize) {
            return (int)( ( fsize + blockMashk ) >> blockBits );
        }
        internal static string UP_HOST = "http://upload.qiniu.com";
        private object mutex = new object();
        private string token;
        private string cloudName;
        private string bucket;
        private long chunkSize = (long)CommonSize.KB512;
        private string bucketId;
        FileState fileState;


        public Task<FileState> SaveAsync(FileState state,
        Stream dataStream,
        String sessionToken,
        IProgress<AVUploadProgressEventArgs> progress,
        CancellationToken cancellationToken) {
            fileState = state;
            cloudName = GetUniqueName();
            return GetQiniuToken(cloudName,CancellationToken.None).ContinueWith(t => {
                return UploadNextChunk(0,dataStream,string.Empty,0,progress);
            }).Unwrap().ContinueWith<FileState>(s => {
                return fileState;
            });
        }
        List<string> block_ctxes = new List<string>();
        Task UploadNextChunk(long completed,Stream dataStream,string context,long offset,IProgress<AVUploadProgressEventArgs> progress) {
            long dataLength = dataStream.Length;
            if (progress != null) {
                lock (mutex) {
                    progress.Report(new AVUploadProgressEventArgs() {
                        Progress = CalcProgress(completed,dataLength)
                    });
                }
            }
            if (completed == dataLength) {
                return QiniuMakeFile(this.token,cloudName,dataLength,block_ctxes.ToArray(),CancellationToken.None);
            }
            if (completed % BLOCKSIZE == 0) {
                if (!string.IsNullOrEmpty(context)) {
                    block_ctxes.Add(context);
                }
                var firstChunkBinary = GetChunkBinary(completed,dataStream);
                var blockSize = ( dataLength - completed ) > BLOCKSIZE ? BLOCKSIZE : ( dataLength - completed );
                return MakeBlock(firstChunkBinary,blockSize).ContinueWith(t => {
                    var dic = AVClient.ReponseResolve(t.Result,CancellationToken.None);
                    var ctx = dic.Item2["ctx"].ToString();
                    offset = long.Parse(dic.Item2["offset"].ToString());
                    completed += firstChunkBinary.Length;
                    UploadNextChunk(completed,dataStream,ctx,offset,progress);
                });
            } else {
                var chunkBinary = GetChunkBinary(completed,dataStream);
                return PutChunk(chunkBinary,context,offset).ContinueWith(t => {
                    var dic = AVClient.ReponseResolve(t.Result,CancellationToken.None);
                    var ctx = dic.Item2["ctx"].ToString();
                    offset = long.Parse(dic.Item2["offset"].ToString());
                    completed += chunkBinary.Length;
                    UploadNextChunk(completed,dataStream,ctx,offset,progress);
                });
            }
        }
        double CalcProgress(double already,double total) {
            var pv = ( 1.0 * already / total );
            return Math.Round(pv,3);
        }
        byte[] GetChunkBinary(long completed,Stream dataStream) {
            if (completed + chunkSize > dataStream.Length) {
                chunkSize = dataStream.Length - completed;
            }
            byte[] chunkBinary = new byte[chunkSize];
            dataStream.Seek(completed,SeekOrigin.Begin);
            dataStream.Read(chunkBinary,0,(int)chunkSize);
            return chunkBinary;
        }

        internal string GetUniqueName() {
            string key = Guid.NewGuid().ToString();//file Key in Qiniu.
            string extension = Path.GetExtension(fileState.Name);
            key += extension;
            return key;
        }
        internal Task<Tuple<HttpStatusCode,IDictionary<string,object>>> GetQiniuToken(string cloudName,CancellationToken cancellationToken) {
            Task<Tuple<HttpStatusCode,IDictionary<string,object>>> rtn;
            string currentSessionToken = AVUser.CurrentSessionToken;
            string str = fileState.Name;
            IDictionary<string,object> parameters = new Dictionary<string,object>();
            parameters.Add("name",str);
            parameters.Add("key",cloudName);
            parameters.Add("__type","File");
            parameters.Add("mime_type",AVFile.GetMIMEType(str));
            parameters.Add("metaData",fileState.MetaData);

            rtn = AVClient.RequestAsync("POST",new Uri("/qiniu",UriKind.Relative),currentSessionToken,parameters,cancellationToken);

            return rtn;
        }
        IList<KeyValuePair<string,string>> GetQiniuRequestHeaders() {
            IList<KeyValuePair<string,string>> makeBlockHeaders = new List<KeyValuePair<string,string>>();

            string authHead = "UpToken " + this.token;
            makeBlockHeaders.Add(new KeyValuePair<string,string>("Authorization",authHead));
            return makeBlockHeaders;
        }

        Task<Tuple<HttpStatusCode,string>> MakeBlock(byte[] firstChunkBinary,long blcokSize = 4194304) {
            MemoryStream firstChunkData = new MemoryStream(firstChunkBinary,0,firstChunkBinary.Length);
            return AVClient.RequestAsync(new Uri(new Uri(UP_HOST) + string.Format("mkblk/{0}",blcokSize)),"POST",GetQiniuRequestHeaders(),firstChunkData,"application/octet-stream",CancellationToken.None);
        }
        Task<Tuple<HttpStatusCode,string>> PutChunk(byte[] chunkBinary,string LastChunkctx,long currentChunkOffsetInBlock) {
            MemoryStream chunkData = new MemoryStream(chunkBinary,0,chunkBinary.Length);
            return AVClient.RequestAsync(new Uri(new Uri(UP_HOST) + string.Format("bput/{0}/{1}",LastChunkctx,
                currentChunkOffsetInBlock)),"POST",
                GetQiniuRequestHeaders(),chunkData,
                "application/octet-stream",CancellationToken.None);
        }
        internal Task<Tuple<HttpStatusCode,string>> QiniuMakeFile(string upToken,string key,long fsize,string[] ctxes,CancellationToken cancellationToken) {
            StringBuilder urlBuilder = new StringBuilder();
            urlBuilder.AppendFormat("{0}/mkfile/{1}",UP_HOST,fsize);
            if (key != null) {
                urlBuilder.AppendFormat("/key/{0}",ToBase64URLSafe(key));
            }
            var metaData = fileState.MetaData;

            StringBuilder sb = new StringBuilder();
            foreach (string _key in metaData.Keys) {
                sb.AppendFormat("/{0}/{1}",_key,ToBase64URLSafe(metaData[_key].ToString()));
            }
            urlBuilder.Append(sb.ToString());

            IList<KeyValuePair<string,string>> headers = new List<KeyValuePair<string,string>>();
            //makeBlockDic.Add("Content-Type", "application/octet-stream");

            string authHead = "UpToken " + upToken;
            headers.Add(new KeyValuePair<string,string>("Authorization",authHead));
            int proCount = ctxes.Length;
            Stream body = new MemoryStream();

            for (int i = 0;i < proCount;i++) {
                byte[] bctx = StringToAscii(ctxes[i]);
                body.Write(bctx,0,bctx.Length);
                if (i != proCount - 1) {
                    body.WriteByte((byte)',');
                }
            }
            body.Seek(0,SeekOrigin.Begin);

            var rtn = AVClient.RequestAsync(new Uri(urlBuilder.ToString()),"POST",headers,body,"text/plain",cancellationToken);
            return rtn;
        }
        internal void MergeFromJSON(FileState state,IDictionary<string,object> jsonData) {
            lock (this.mutex) {
                string url = jsonData["url"] as string;
                state.Url = new Uri(url,UriKind.Absolute);
                this.bucketId = FetchBucketId(url);
                this.token = jsonData["token"] as string;
                this.bucket = jsonData["bucket"] as string;
                state.ObjectId = jsonData["objectId"] as string;
            }
        }

        string FetchBucketId(string url) {
            var elements = url.Split('/');

            return elements[elements.Length - 1];
        }
        public static byte[] StringToAscii(string s) {
            byte[] retval = new byte[s.Length];
            for (int ix = 0;ix < s.Length;++ix) {
                char ch = s[ix];
                if (ch <= 0x7f)
                    retval[ix] = (byte)ch;
                else
                    retval[ix] = (byte)'?';
            }
            return retval;
        }
        public static string ToBase64URLSafe(string str) {
            return Encode(str);
        }
        public static string Encode(byte[] bs) {
            if (bs == null || bs.Length == 0)
                return "";
            string encodedStr = Convert.ToBase64String(bs);
            encodedStr = encodedStr.Replace('+','-').Replace('/','_');
            return encodedStr;
        }
        public static string Encode(string text) {
            if (String.IsNullOrEmpty(text))
                return "";
            byte[] bs = Encoding.UTF8.GetBytes(text);
            string encodedStr = Convert.ToBase64String(bs);
            encodedStr = encodedStr.Replace('+','-').Replace('/','_');
            return encodedStr;
        }
    }
}
