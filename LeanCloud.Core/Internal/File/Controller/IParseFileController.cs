// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This Source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this Source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud.Core.Internal
{
    public interface IAVFileController
    {
        Task<FileState> SaveAsync(FileState state,
            Stream dataStream,
            String sessionToken,
            IProgress<AVUploadProgressEventArgs> progress,
            CancellationToken cancellationToken);

        Task DeleteAsync(FileState state,
         string sessionToken,
         CancellationToken cancellationToken);

        Task<FileState> GetAsync(string objectId,
            string sessionToken,
            CancellationToken cancellationToken);
    }
}
