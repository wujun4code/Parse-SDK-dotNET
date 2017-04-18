// Copyright (c) 2015-present, LeanCloud, LLC.  All rights reserved.  This Source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this Source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace LeanCloud {
  public partial class AVInstallation : AVObject {
    /// <summary>
    /// iOS Badge.
    /// </summary>
    [AVFieldName("badge")]
    public int Badge {
      get {
        return GetProperty<int>("Badge");
      }
      set {
        int badge = value;
        SetProperty<int>(badge, "Badge");
      }
    }
  }
}
