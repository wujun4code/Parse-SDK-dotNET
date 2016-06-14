// Copyright (c) 2015-present, Parse, LLC  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System.Runtime.CompilerServices;

// Platform specific libraries can see LeanCloud internals.
[assembly: InternalsVisibleTo("LeanCloud")]

// Internal visibility for platform-specific libraries.
[assembly: InternalsVisibleTo("LeanCloud.WinRT")]
[assembly: InternalsVisibleTo("LeanCloud.NetFx45")]
[assembly: InternalsVisibleTo("LeanCloud.Phone")]
[assembly: InternalsVisibleTo("LeanMessage.NetFx45")]
// Internal visibility for test libraries.
[assembly: InternalsVisibleTo("LeanCloud.Integration.WinRT")]
[assembly: InternalsVisibleTo("LeanCloud.Integration.NetFx45")]
[assembly: InternalsVisibleTo("LeanCloud.Integration.Phone")]


[assembly: InternalsVisibleTo("LeanCloud.Unit.NetFx45")]
[assembly: InternalsVisibleTo("Unit.Test.452")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

// Internal visibility for sample projects
[assembly: InternalsVisibleTo("LeanCloudPushSample")]
[assembly: InternalsVisibleTo("LeanCloudPushSample")]


#if MONO
[assembly: InternalsVisibleTo("AVTestIntegrationiOS")]
[assembly: InternalsVisibleTo("AVTest.Integration.Android")]
#endif

#if UNITY
[assembly: InternalsVisibleTo("AVTest.Integration.Unity")]
#endif