// Copyright (c) 2015-present, LeanCloud, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Threading.Tasks;
using System.Threading;

namespace LeanCloud.Internal {
  /// <summary>
  /// Config controller.
  /// </summary>
  internal class AVConfigController : IAVConfigController {
    /// <summary>
    /// Initializes a new instance of the <see cref="AVConfigController"/> class.
    /// </summary>
    public AVConfigController() {
      CurrentConfigController = new AVCurrentConfigController();
    }

    public IAVCurrentConfigController CurrentConfigController { get; internal set; }

    public Task<AVConfig> FetchConfigAsync(String sessionToken, CancellationToken cancellationToken) {
      var command = new AVCommand("/config",
          method: "GET",
          sessionToken: sessionToken,
          data: null);

      return AVClient.AVCommandRunner.RunCommandAsync(command, cancellationToken: cancellationToken).OnSuccess(task => {
        cancellationToken.ThrowIfCancellationRequested();
        return new AVConfig(task.Result.Item2);
      }).OnSuccess(task => {
        cancellationToken.ThrowIfCancellationRequested();
        CurrentConfigController.SetCurrentConfigAsync(task.Result);
        return task;
      }).Unwrap();
    }
  }
}
