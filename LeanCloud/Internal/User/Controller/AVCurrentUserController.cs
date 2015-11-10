// Copyright (c) 2015-present, Parse, LLC  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud.Internal {
  class AVCurrentUserController : IAVCurrentUserController {
    private readonly object mutex = new object();
    private readonly TaskQueue taskQueue = new TaskQueue();

    private AVUser currentUser;
    internal AVUser CurrentUser {
      get {
        lock (mutex) {
          return currentUser;
        }
      }
      set {
        lock (mutex) {
          currentUser = value;
        }
      }
    }

    public Task SetAsync(AVUser user, CancellationToken cancellationToken) {
      return taskQueue.Enqueue(toAwait => {
        return toAwait.ContinueWith(_ => {
          if (user == null) {
            AVClient.ApplicationSettings.Remove("CurrentUser");
          } else {
            // TODO (hallucinogen): we need to use AVCurrentCoder instead of this janky encoding
            var data = user.ServerDataToJSONObjectForSerialization();
            data["objectId"] = user.ObjectId;
            if (user.CreatedAt != null) {
              data["createdAt"] = user.CreatedAt.Value.ToString(AVClient.DateFormatString);
            }
            if (user.UpdatedAt != null) {
              data["updatedAt"] = user.UpdatedAt.Value.ToString(AVClient.DateFormatString);
            }

            AVClient.ApplicationSettings["CurrentUser"] = Json.Encode(data);
          }
          CurrentUser = user;
        });
      }, cancellationToken);
    }

    public Task<AVUser> GetAsync(CancellationToken cancellationToken) {
      AVUser cachedCurrent;

      lock (mutex) {
        cachedCurrent = CurrentUser;
      }

      if (cachedCurrent != null) {
        return Task<AVUser>.FromResult(cachedCurrent);
      }

      return taskQueue.Enqueue(toAwait => {
        return toAwait.ContinueWith(t => {
          object temp;
          AVClient.ApplicationSettings.TryGetValue("CurrentUser", out temp);
          var userDataString = temp as string;
          AVUser user = null;
          if (userDataString != null) {
            var userData =  Json.Parse(userDataString) as IDictionary<string, object>;
            user = AVObject.CreateWithoutData<AVUser>(null);
            user.HandleFetchResult(AVObjectCoder.Instance.Decode(userData, AVDecoder.Instance));
          }

          CurrentUser = user;
          return user;
        });
      }, cancellationToken);
    }

    public Task<bool> ExistsAsync(CancellationToken cancellationToken) {
      if (CurrentUser != null) {
        return Task<bool>.FromResult(true);
      }

      return taskQueue.Enqueue(toAwait => {
        return toAwait.ContinueWith(t => AVClient.ApplicationSettings.ContainsKey("CurrentUser"));
      }, cancellationToken);
    }

    public bool IsCurrent(AVUser user) {
      lock (mutex) {
        return CurrentUser == user;
      }
    }

    public void ClearFromMemory() {
      CurrentUser = null;
    }

    public void ClearFromDisk() {
      lock (mutex) {
        ClearFromMemory();

        AVClient.ApplicationSettings.Remove("CurrentUser");
      }
    }

    public Task<string> GetCurrentSessionTokenAsync(CancellationToken cancellationToken) {
      return GetAsync(cancellationToken).OnSuccess(t => {
        var user = t.Result;
        return user == null ? null : user.SessionToken;
      });
    }

    public Task LogOutAsync(CancellationToken cancellationToken) {
      return taskQueue.Enqueue(toAwait => {
        return toAwait.ContinueWith(_ => GetAsync(cancellationToken)).Unwrap().OnSuccess(t => {
          ClearFromDisk();
        });
      }, cancellationToken);
    }
  }
}
