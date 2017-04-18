// Copyright (c) 2015-present, LeanCloud, LLC.  All rights reserved.  This Source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this Source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;
using System.Collections.Generic;

namespace LeanCloud.Push.Internal {
  public interface IAVPushChannelsController {
    Task SubscribeAsync(IEnumerable<string> channels, CancellationToken cancellationToken);
    Task UnsubscribeAsync(IEnumerable<string> channels, CancellationToken cancellationToken);
  }
}
