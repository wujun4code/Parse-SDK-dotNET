// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This Source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this Source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using LeanCloud.Core.Internal;
using System;

namespace LeanCloud.Analytics.Internal {
  public interface IAVAnalyticsPlugins {
    void Reset();

    IAVCorePlugins CorePlugins { get; }
    IAVAnalyticsController AnalyticsController { get; }
  }
}