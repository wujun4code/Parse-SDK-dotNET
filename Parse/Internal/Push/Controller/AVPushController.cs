// Copyright (c) 2015-present, LeanCloud, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace LeanCloud.Internal {
  internal class AVPushController : IAVPushController {
    public Task SendPushNotificationAsync(IPushState state, String sessionToken, CancellationToken cancellationToken) {
      var command = new AVCommand("/1.1/push",
          method: "POST",
          sessionToken: sessionToken,
          data: AVPushEncoder.Instance.Encode(state));

      return AVClient.AVCommandRunner.RunCommandAsync(command, cancellationToken: cancellationToken);
    }
  }
}
