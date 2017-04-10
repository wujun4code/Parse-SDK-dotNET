// Copyright (c) 2015-present, LeanCloud, LLC.  All rights reserved.  This Source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this Source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;

namespace LeanCloud.Push.Internal {
  public interface IAVState {
    AVQuery<AVInstallation> Query { get; }
    IEnumerable<string> Channels { get; }
    DateTime? Expiration { get; }
    TimeSpan? ExpirationInterval { get; }
    DateTime? PushTime { get; }
    IDictionary<string, object> Data { get; }
    String Alert { get; }

    IAVState MutatedClone(Action<MutableAVState> func);
  }
}
