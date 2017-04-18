// Copyright (c) 2015-present, LeanCloud, LLC.  All rights reserved.  This Source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this Source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Linq;
using System.Collections.Generic;
using LeanCloud.Storage.Internal;

namespace LeanCloud.Push.Internal {
  public class MutableAVState : IAVState {
    public AVQuery<AVInstallation> Query { get; set; }
    public IEnumerable<string> Channels { get; set; }
    public DateTime? Expiration { get; set; }
    public TimeSpan? ExpirationInterval { get; set; }
    public DateTime? PushTime { get; set; }
    public IDictionary<string, object> Data { get; set; }
    public String Alert { get; set; }

    public IAVState MutatedClone(Action<MutableAVState> func) {
      MutableAVState clone = MutableClone();
      func(clone);
      return clone;
    }

    protected virtual MutableAVState MutableClone() {
      return new MutableAVState {
        Query = Query,
        Channels = Channels == null ? null: new List<string>(Channels),
        Expiration = Expiration,
        ExpirationInterval = ExpirationInterval,
        PushTime = PushTime,
        Data = Data == null ? null : new Dictionary<string, object>(Data),
        Alert = Alert
      };
    }

    public override bool Equals(object obj) {
      if (obj == null || !(obj is MutableAVState)) {
        return false;
      }

      var other = obj as MutableAVState;
      return Object.Equals(this.Query, other.Query) &&
             this.Channels.CollectionsEqual(other.Channels) &&
             Object.Equals(this.Expiration, other.Expiration) &&
             Object.Equals(this.ExpirationInterval, other.ExpirationInterval) &&
             Object.Equals(this.PushTime, other.PushTime) &&
             this.Data.CollectionsEqual(other.Data) &&
             Object.Equals(this.Alert, other.Alert);
    }

    public override int GetHashCode() {
      // TODO (richardross): Implement this.
      return 0;
    }
  }
}
