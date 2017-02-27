// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using LeanCloud.Storage.Internal;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("LeanCloud.Storage")]
[assembly: AssemblyDescription("Makes accessing services from LeanCloud native and straightforward.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("LeanCloud")]
[assembly: AssemblyCopyright("Copyright © LeanCloud 2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(true)]
[assembly: AssemblyVersion(AVVersionInfo.Version)]
[assembly: AssemblyFileVersion(AVVersionInfo.Version)]


class AVVersionInfo
{
	public const string Version = "2.0.1.0";
}