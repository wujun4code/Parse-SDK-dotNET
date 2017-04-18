// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This Source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this Source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud.Analytics.Internal
{
    public interface IAVAnalyticsController
    {
        Task TrackEventAsync(string name,
            IDictionary<string, string> dimensions,
            string sessionToken,
            CancellationToken cancellationToken);

        Task TrackAppOpenedAsync(string pushHash,
            string sessionToken,
            CancellationToken cancellationToken);

        Task<IDictionary<string,object>> GetPolicyAsync(string sessionToken, CancellationToken cancellationToken);

        Task<bool> SendAsync(IDictionary<string,object> analyticsData,string sessionToken, CancellationToken cancellationToken);
        
    }
}
