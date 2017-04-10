// Copyright (c) 2015-present, LeanCloud, LLC.  All rights reserved.  This Source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this Source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using LeanCloud.Storage.Internal;
using LeanCloud.Push.Internal;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("LeanCloud")]
[assembly: AssemblyDescription("Makes accessing services from LeanCloud native and straightforward.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("LeanCloud")]
[assembly: AssemblyCopyright("Copyright © LeanCloud 2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(true)]

[assembly: AVModule(typeof(AVPushModule))]
