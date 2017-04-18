// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This Source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this Source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeanCloud.Core.Internal;
using LeanCloud.Storage.Internal;

namespace LeanCloud.Analytics.Internal
{
    public class AVAnalyticsController : IAVAnalyticsController
    {
        private readonly IAVCommandRunner commandRunner;

        public AVAnalyticsController(IAVCommandRunner commandRunner)
        {
            this.commandRunner = commandRunner;
        }

        public Task TrackEventAsync(string name,
            IDictionary<string, string> dimensions,
            string sessionToken,
            CancellationToken cancellationToken)
        {
            IDictionary<string, object> data = new Dictionary<string, object> {
                { "at", DateTime.Now },
                { "name", name },
            };
            if (dimensions != null)
            {
                data["dimensions"] = dimensions;
            }

            var command = new AVCommand("events/" + name,
                method: "POST",
                sessionToken: sessionToken,
                data: PointerOrLocalIdEncoder.Instance.Encode(data) as IDictionary<string, object>);

            return commandRunner.RunCommandAsync(command, cancellationToken: cancellationToken);
        }

        public Task TrackAppOpenedAsync(string pushHash,
            string sessionToken,
            CancellationToken cancellationToken)
        {
            IDictionary<string, object> data = new Dictionary<string, object> {
                { "at", DateTime.Now }
            };
            if (pushHash != null)
            {
                data["push_hash"] = pushHash;
            }

            var command = new AVCommand("events/AppOpened",
                method: "POST",
                sessionToken: sessionToken,
                data: PointerOrLocalIdEncoder.Instance.Encode(data) as IDictionary<string, object>);

            return commandRunner.RunCommandAsync(command, cancellationToken: cancellationToken);
        }

        public Task<IDictionary<string, object>> GetPolicyAsync(string sessionToken, CancellationToken cancellationToken)
        {
            var command = new AVCommand(string.Format("statistics/apps/{0}/sendPolicy", AVClient.CurrentConfiguration.ApplicationId),
               method: "GET",
               sessionToken: sessionToken,
               data: null);

            return commandRunner.RunCommandAsync(command, cancellationToken: cancellationToken).OnSuccess(t =>
            {
                return t.Result.Item2;
            });
        }

        public Task<bool> SendAsync(IDictionary<string, object> analyticsData,string sessionToken, CancellationToken cancellationToken)
        {
            var command = new AVCommand("stats/collect",
              method: "POST",
              sessionToken: sessionToken,
              data: analyticsData);

            return commandRunner.RunCommandAsync(command, cancellationToken: cancellationToken).OnSuccess(t =>
            {
                return t.Result.Item1 == System.Net.HttpStatusCode.OK;
            });
        }

    }
}
