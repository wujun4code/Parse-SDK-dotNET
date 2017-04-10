// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This Source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this Source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;

namespace LeanCloud.Core.Internal {
  /// <summary>
  /// A <see cref="ParseEncoder"/> that throws an exception if it attempts to encode
  /// a <see cref="AVObject"/>
  /// </summary>
  public class NoObjectsEncoder : AVEncoder {
    // This class isn't really a Singleton, but since it has no state, it's more efficient to get
    // the default instance.
    private static readonly NoObjectsEncoder instance = new NoObjectsEncoder();
    public static NoObjectsEncoder Instance {
      get {
        return instance;
      }
    }

    protected override IDictionary<string, object> EncodeParseObject(AVObject value) {
      throw new ArgumentException("AVObjects not allowed here.");
    }
  }
}
