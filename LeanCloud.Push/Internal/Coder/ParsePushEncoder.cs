// Copyright (c) 2015-present, LeanCloud, LLC.  All rights reserved.  This Source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this Source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Linq;
using System.Collections.Generic;
using LeanCloud.Storage.Internal;
using LeanCloud.Core.Internal;

namespace LeanCloud.Push.Internal {
  public class AVPushEncoder {
    private static readonly AVPushEncoder instance = new AVPushEncoder();
    public static AVPushEncoder Instance {
      get {
        return instance;
      }
    }

    private AVPushEncoder() { }

    public IDictionary<string, object> Encode(IAVState state) {
      if (state.Alert == null && state.Data == null) {
        throw new InvalidOperationException("A push must have either an Alert or Data");
      }
      if (state.Channels == null && state.Query == null) {
        throw new InvalidOperationException("A push must have either Channels or a Query");
      }

      var data = state.Data ?? new Dictionary<string, object> { { "alert", state.Alert } };
      var query = state.Query ?? AVInstallation.Query;
      if (state.Channels != null) {
        query = query.WhereContainedIn("channels", state.Channels);
      }
      var payload = new Dictionary<string, object> {
        { "data", data },
        { "where", query.BuildParameters().GetOrDefault("where", new Dictionary<string, object>()) },
      };
      if (state.Expiration.HasValue) {
        payload["expiration_time"] = state.Expiration.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");
      } else if (state.ExpirationInterval.HasValue) {
        payload["expiration_interval"] = state.ExpirationInterval.Value.TotalSeconds;
      }
      if (state.PushTime.HasValue) {
        payload["push_time"] = state.PushTime.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
      }

      return payload;
    }
  }
}
