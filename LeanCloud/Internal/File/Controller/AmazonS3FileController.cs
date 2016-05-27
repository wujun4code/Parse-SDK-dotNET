using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud.Internal
{
    internal class AmazonS3FileController : IAVFileController
    {
        public Task<FileState> SaveAsync(FileState state, 
            Stream dataStream, 
            string sessionToken, 
            IProgress<AVUploadProgressEventArgs> progress, 
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
