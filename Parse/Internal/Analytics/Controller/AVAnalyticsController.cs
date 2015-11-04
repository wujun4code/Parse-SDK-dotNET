// Copyright (c) 2015-present, LeanCloud, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace LeanCloud.Internal {
  internal class AVAnalyticsController : IAVAnalyticsController {
    private readonly IAVCommandRunner commandRunner;

    internal AVAnalyticsController(IAVCommandRunner commandRunner) {
      this.commandRunner = commandRunner;
    }

    public Task TrackEventAsync(string name,
        IDictionary<string, string> dimensions,
        string sessionToken,
        CancellationToken cancellationToken) {
      IDictionary<string, object> data = new Dictionary<string, object> {
        { "at", DateTime.Now },
        { "name", name },
      };
      if (dimensions != null) {
        data["dimensions"] = dimensions;
      }

      var command = new AVCommand("/1/events/" + name,
          method: "POST",
          sessionToken: sessionToken,
          data: PointerOrLocalIdEncoder.Instance.Encode(data) as IDictionary<string, object>);

      return commandRunner.RunCommandAsync(command, cancellationToken: cancellationToken);
    }

    public Task TrackAppOpenedAsync(string pushHash,
        string sessionToken,
        CancellationToken cancellationToken) {
      IDictionary<string, object> data = new Dictionary<string, object> {
        { "at", DateTime.Now }
      };
      if (pushHash != null) {
        data["push_hash"] = pushHash;
      }

      var command = new AVCommand("/1/events/AppOpened",
          method: "POST",
          sessionToken: sessionToken,
          data: PointerOrLocalIdEncoder.Instance.Encode(data) as IDictionary<string, object>);

      return commandRunner.RunCommandAsync(command, cancellationToken: cancellationToken);
    }
  }
}
