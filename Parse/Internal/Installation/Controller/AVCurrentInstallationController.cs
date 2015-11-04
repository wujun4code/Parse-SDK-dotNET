// Copyright (c) 2015-present, LeanCloud, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud.Internal {
  internal class AVCurrentInstallationController : IAVCurrentInstallationController {
    private readonly object mutex = new object();
    private readonly TaskQueue taskQueue = new TaskQueue();
    private readonly IInstallationIdController installationIdController;

    public AVCurrentInstallationController(IInstallationIdController installationIdController) {
      this.installationIdController = installationIdController;
    }

    private AVInstallation currentInstallation;
    internal AVInstallation CurrentInstallation {
      get {
        lock (mutex) {
          return currentInstallation;
        }
      }
      set {
        lock (mutex) {
          currentInstallation = value;
        }
      }
    }

    public Task SetAsync(AVInstallation installation, CancellationToken cancellationToken) {
      return taskQueue.Enqueue(toAwait => {
        return toAwait.ContinueWith(_ => {
          if (installation == null) {
            AVClient.ApplicationSettings.Remove("CurrentInstallation");
          } else {
            // TODO (hallucinogen): we need to use AVCurrentCoder instead of this janky encoding
            var data = installation.ServerDataToJSONObjectForSerialization();
            data["objectId"] = installation.ObjectId;
            if (installation.CreatedAt != null) {
              data["createdAt"] = installation.CreatedAt.Value.ToString(AVClient.DateFormatString);
            }
            if (installation.UpdatedAt != null) {
              data["updatedAt"] = installation.UpdatedAt.Value.ToString(AVClient.DateFormatString);
            }

            AVClient.ApplicationSettings["CurrentInstallation"] = Json.Encode(data);
          }
          CurrentInstallation = installation;
        });
      }, cancellationToken);
    }

    public Task<AVInstallation> GetAsync(CancellationToken cancellationToken) {
      AVInstallation cachedCurrent;
      cachedCurrent = CurrentInstallation;

      if (cachedCurrent != null) {
        return Task<AVInstallation>.FromResult(cachedCurrent);
      }

      return taskQueue.Enqueue(toAwait => {
        return toAwait.ContinueWith(t => {
          object temp;
          AVClient.ApplicationSettings.TryGetValue("CurrentInstallation", out temp);
          var installationDataString = temp as string;
          AVInstallation installation = null;
          if (installationDataString != null) {
            var installationData = AVClient.DeserializeJsonString(installationDataString);
            installation = AVObject.CreateWithoutData<AVInstallation>(null);
            installation.HandleFetchResult(AVObjectCoder.Instance.Decode(installationData, AVDecoder.Instance));
          } else {
            installation = AVObject.Create<AVInstallation>();
            installation.SetIfDifferent("installationId" , installationIdController.Get().ToString());
          }

          CurrentInstallation = installation;
          return installation;
        });
      }, cancellationToken);
    }

    public Task<bool> ExistsAsync(CancellationToken cancellationToken) {
      if (CurrentInstallation != null) {
        return Task<bool>.FromResult(true);
      }

      return taskQueue.Enqueue(toAwait => {
        return toAwait.ContinueWith(t => AVClient.ApplicationSettings.ContainsKey("CurrentInstallation"));
      }, cancellationToken);
    }

    public bool IsCurrent(AVInstallation installation) {
      return CurrentInstallation == installation;
    }

    public void ClearFromMemory() {
      CurrentInstallation = null;
    }

    public void ClearFromDisk() {
      ClearFromMemory();

      AVClient.ApplicationSettings.Remove("CurrentInstallation");
    }
  }
}
