// Copyright (c) 2015-present, Parse, LLC  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

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
      StringPayload = AVClient.SerializeJsonString(payload);
#endif
    }

// Obj-C type -> .NET type is impossible to do flawlessly (especially
// on NSNumber). We can't transform NSDictionary into string because of this reason. 
#if !IOS
    internal AVPushNotificationEventArgs(string stringPayload) {
      StringPayload = stringPayload;

      Payload = AVClient.DeserializeJsonString(stringPayload);
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
