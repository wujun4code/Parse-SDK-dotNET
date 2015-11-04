// Copyright (c) 2015-present, LeanCloud, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;


namespace LeanCloud.Internal {
  internal class AVCommandRunner : IAVCommandRunner {
    private readonly IHttpClient httpClient;
    public AVCommandRunner(IHttpClient httpClient) {
      this.httpClient = httpClient;
    }

    public Task<Tuple<HttpStatusCode, IDictionary<string, object>>> RunCommandAsync(AVCommand command,
        IProgress<AVUploadProgressEventArgs> uploadProgress = null,
        IProgress<AVDownloadProgressEventArgs> downloadProgress = null,
        CancellationToken cancellationToken = default(CancellationToken)) {
      return httpClient.ExecuteAsync(command, uploadProgress, downloadProgress, cancellationToken).OnSuccess(t => {
        cancellationToken.ThrowIfCancellationRequested();

        var response = t.Result;
        var contentString = response.Item2;
        int responseCode = (int)response.Item1;
        if (responseCode >= 500) {
          // Server error, return InternalServerError.
          throw new AVException(AVException.ErrorCode.InternalServerError, response.Item2);
        } else if (contentString != null) {
          IDictionary<string, object> contentJson = null;
          try {
            if (contentString.StartsWith("[")) {
              var arrayJson = Json.AV(contentString);
              contentJson = new Dictionary<string, object> { { "results", arrayJson } };
            } else {
							contentJson = Json.AV(contentString) as IDictionary<string, object>;
            }
          } catch (Exception e) {
            throw new AVException(AVException.ErrorCode.OtherCause,
                "Invalid response from server", e);
          }
          if (responseCode < 200 || responseCode > 299) {
            int code = (int)(contentJson.ContainsKey("code") ? (long)contentJson["code"] : (int)AVException.ErrorCode.OtherCause);
            string error = contentJson.ContainsKey("error") ?
                contentJson["error"] as string :
                contentString;
            throw new AVException((AVException.ErrorCode)code, error);
          }
          return new Tuple<HttpStatusCode, IDictionary<string, object>>(response.Item1,
              contentJson);
        }
        return new Tuple<HttpStatusCode, IDictionary<string, object>>(response.Item1, null);
      });
    }
  }
}
