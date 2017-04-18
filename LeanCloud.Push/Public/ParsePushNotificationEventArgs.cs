// Copyright (c) 2015-present, LeanCloud, LLC.  All rights reserved.  This Source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this Source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using LeanCloud.Storage.Internal;
using System;
using System.Collections.Generic;

namespace LeanCloud {
  /// <summary>
  /// A wrapper around LeanCloud push notification payload.
  /// </summary>
  public class AVPushNotificationEventArgs : EventArgs {
    internal AVPushNotificationEventArgs(IDictionary<string, object> payload) {
      Payload = payload;

#if !IOS
      StringPayload = Json.Encode(payload);
#endif
    }

// TODO: (richardross) investigate this.
// Obj-C type -> .NET type is impossible to do flawlessly (especially
// on NSNumber). We can't transform NSDictionary into string because of this reason.
#if !IOS
    internal AVPushNotificationEventArgs(string stringPayload) {
      StringPayload = stringPayload;

      Payload = Json.Parse(stringPayload) as IDictionary<string, object>;
    }
#endif

    /// <summary>
    /// The payload of the push notification as <c>IDictionary</c>.
    /// </summary>
    public IDictionary<string, object> Payload { get; internal set; }

    /// <summary>
    /// The payload of the push notification as <c>string</c>.
    /// </summary>
    public string StringPayload { get; internal set; }
  }
}
