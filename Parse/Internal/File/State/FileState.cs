// Copyright (c) 2015-present, LeanCloud, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;

namespace LeanCloud.Internal {
  internal class FileState {
    public string Name { get; internal set; }
    public string MimeType { get; internal set; }
    public Uri Url { get; internal set; }
  }
}
