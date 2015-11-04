// Copyright (c) 2015-present, LeanCloud, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeanCloud.Internal;

namespace LeanCloud.Internal {
  internal class AVCorePlugins {
    private static readonly AVCorePlugins instance = new AVCorePlugins();
    public static AVCorePlugins Instance {
      get {
        return instance;
      }
    }

    private readonly object mutex = new object();

    #region Server Controllers

    private IAVAnalyticsController analyticsController;
    private IAVCloudCodeController cloudCodeController;
    private IAVConfigController configController;
    private IAVFileController fileController;
    private IAVObjectController objectController;
    private IAVQueryController queryController;
    private IAVSessionController sessionController;
    private IAVUserController userController;
    private IAVPushController pushController;
    private IAVPushChannelsController pushChannelsController;

    #endregion

    #region Current Instance Controller

    private IInstallationIdController installationIdController;
    private IAVCurrentInstallationController currentInstallationController;
    private IAVCurrentUserController currentUserController;

    #endregion

    internal void Reset() {
      lock (mutex) {
        AnalyticsController = null;
        CloudCodeController = null;
        FileController = null;
        ObjectController = null;
        SessionController = null;
        UserController = null;

        CurrentInstallationController = null;
        CurrentUserController = null;
      }
    }

    public IAVAnalyticsController AnalyticsController {
      get {
        lock (mutex) {
          analyticsController = analyticsController ?? new AVAnalyticsController(AVClient.AVCommandRunner);
          return analyticsController;
        }
      }
      internal set {
        lock (mutex) {
          analyticsController = value;
        }
      }
    }

    public IAVCloudCodeController CloudCodeController {
      get {
        lock (mutex) {
          cloudCodeController = cloudCodeController ?? new AVCloudCodeController(AVClient.AVCommandRunner);
          return cloudCodeController;
        }
      }
      internal set {
        lock (mutex) {
          cloudCodeController = value;
        }
      }
    }

    public IAVFileController FileController {
      get {
        lock (mutex) {
          fileController = fileController ?? new AVFileController(AVClient.AVCommandRunner);
          return fileController;
        }
      }
      internal set {
        lock (mutex) {
          fileController = value;
        }
      }
    }

    public IAVConfigController ConfigController {
      get {
        lock (mutex) {
          if (configController == null) {
            configController = new AVConfigController();
          }
          return configController;
        }
      }
      internal set {
        lock (mutex) {
          configController = value;
        }
      }
    }

    public IAVObjectController ObjectController {
      get {
        lock (mutex) {
          objectController = objectController ?? new AVObjectController(AVClient.AVCommandRunner);
          return objectController;
        }
      }
      internal set {
        lock (mutex) {
          objectController = value;
        }
      }
    }

    public IAVQueryController QueryController {
      get {
        lock (mutex) {
          if (queryController == null) {
            queryController = new AVQueryController();
          }
          return queryController;
        }
      }

      internal set {
        lock (mutex) {
          queryController = value;
        }
      }
    }

    public IAVSessionController SessionController {
      get {
        lock (mutex) {
          sessionController = sessionController ?? new AVSessionController(AVClient.AVCommandRunner);
          return sessionController;
        }
      }

      internal set {
        lock (mutex) {
          sessionController = value;
        }
      }
    }

    public IAVUserController UserController {
      get {
        lock (mutex) {
          userController = userController ?? new AVUserController(AVClient.AVCommandRunner);
          return userController;
        }
      }
      internal set {
        lock (mutex) {
          userController = value;
        }
      }
    }

    public IAVPushController PushController {
      get {
        lock (mutex) {
          pushController = pushController ?? new AVPushController();
          return pushController;
        }
      }
      internal set {
        lock (mutex) {
          pushController = value;
        }
      }
    }

    public IAVPushChannelsController PushChannelsController {
      get {
        lock (mutex) {
          pushChannelsController = pushChannelsController ?? new AVPushChannelsController();
          return pushChannelsController;
        }
      }
      internal set {
        lock (mutex) {
          pushChannelsController = value;
        }
      }
    }

    public IInstallationIdController InstallationIdController {
      get {
        lock (mutex) {
          installationIdController = installationIdController ?? new InstallationIdController();
          return installationIdController;
        }
      }
      internal set {
        lock (mutex) {
          installationIdController = value;
        }
      }
    }

    public IAVCurrentInstallationController CurrentInstallationController {
      get {
        lock (mutex) {
          if (currentInstallationController == null) {
            currentInstallationController = new AVCurrentInstallationController(InstallationIdController);
          }
          return currentInstallationController;
        }
      }
      internal set {
        lock (mutex) {
          currentInstallationController = value;
        }
      }
    }

    public IAVCurrentUserController CurrentUserController {
      get {
        lock (mutex) {
          currentUserController = currentUserController ?? new AVCurrentUserController();
          return currentUserController;
        }
      }
      internal set {
        lock (mutex) {
          currentUserController = value;
        }
      }
    }
  }
}
