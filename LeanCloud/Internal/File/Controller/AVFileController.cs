// Copyright (c) 2015-present, Parse, LLC  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud.Internal
{
    internal class AVFileController : IAVFileController
    {
        private readonly IAVCommandRunner commandRunner;

        internal AVFileController(IAVCommandRunner commandRunner)
        {
            this.commandRunner = commandRunner;
        }

        public Task<FileState> SaveAsync(FileState state,
            Stream dataStream,
            String sessionToken,
            IProgress<AVUploadProgressEventArgs> progress,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (state.Url != null)
            {
                // !isDirty
                return Task<FileState>.FromResult(state);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                var tcs = new TaskCompletionSource<FileState>();
                tcs.TrySetCanceled();
                return tcs.Task;
            }

            var oldPosition = dataStream.Position;
            var command = new AVCommand("/files/" + state.Name,
                method: "POST",
                sessionToken: sessionToken,
                contentType: state.MimeType,
                stream: dataStream);

            return commandRunner.RunCommandAsync(command,
                uploadProgress: progress,
                cancellationToken: cancellationToken).OnSuccess(uploadTask =>
                {
                    var result = uploadTask.Result;
                    var jsonData = result.Item2;
                    cancellationToken.ThrowIfCancellationRequested();

                    return new FileState
                    {
                        Name = jsonData["name"] as string,
                        Url = new Uri(jsonData["url"] as string, UriKind.Absolute),
                        MimeType = state.MimeType
                    };
                }).ContinueWith(t =>
                {
              // Rewind the stream on failure or cancellation (if possible)
              if ((t.IsFaulted || t.IsCanceled) && dataStream.CanSeek)
                    {
                        dataStream.Seek(oldPosition, SeekOrigin.Begin);
                    }
                    return t;
                }).Unwrap();
        }


        internal static Task<Tuple<HttpStatusCode, IDictionary<string, object>>> GetFileToken(FileState fileState,CancellationToken cancellationToken)
        {
            Task<Tuple<HttpStatusCode, IDictionary<string, object>>> rtn;
            string currentSessionToken = AVUser.CurrentSessionToken;
            string str = fileState.Name;
            IDictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("name", str);
            parameters.Add("key", GetUniqueName(fileState));
            parameters.Add("__type", "File");
            parameters.Add("mime_type", AVFile.GetMIMEType(str));
            parameters.Add("metaData", fileState.MetaData);

            rtn = AVClient.RequestAsync("POST", new Uri("/fileTokens", UriKind.Relative), currentSessionToken, parameters, cancellationToken);

            return rtn;
        }
        internal static double CalcProgress(double already, double total)
        {
            var pv = (1.0 * already / total);
            return Math.Round(pv, 3);
        }
        internal static string GetUniqueName(FileState fileState)
        {
            string key = Random(12);
            string extension = Path.GetExtension(fileState.Name);
            key += extension;
            fileState.CloudName = key;
            return key;
        }
        internal static string Random(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
