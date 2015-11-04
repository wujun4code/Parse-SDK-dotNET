// Copyright (c) 2015-present, LeanCloud, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LeanCloud {
  /// <summary>
  /// Mandatory MonoBehaviour for scenes that use LeanCloud. Set the application ID and .NET key
  /// in the editor.
  /// </summary>
  // TODO (hallucinogen): somehow because of Push, we need this class to be added in a GameObject
  // called `AVInitializeBehaviour`. We might want to fix this.
  public class AVInitializeBehaviour : MonoBehaviour {
    private static bool isInitialized = false;

    /// <summary>
    /// The LeanCloud applicationId used in this app. You can get this value from the LeanCloud website.
    /// </summary>
    [SerializeField]
    public string applicationID;

    /// <summary>
    /// The LeanCloud dotnetKey used in this app. You can get this value from the LeanCloud website.
    /// </summary>
    [SerializeField]
    public string dotnetKey;

    /// <summary>
    /// Initializes the LeanCloud SDK and begins running network requests created by LeanCloud.
    /// </summary>
    public virtual void Awake() {
      Initialize();
      // Force the name to be `AVInitializeBehaviour` in runtime.
      gameObject.name = "AVInitializeBehaviour";

      if (PlatformHooks.IsIOS) {
        PlatformHooks.RegisterDeviceTokenRequest((deviceToken) => {
          if (deviceToken != null) {
            AVInstallation installation = AVInstallation.CurrentInstallation;
            installation.SetDeviceTokenFromData(deviceToken);

            // Optimistically assume this will finish.
            installation.SaveAsync();
          }
        });
      }
    }

    public void OnApplicationPause(bool paused) {
      if (PlatformHooks.IsAndroid) {
        PlatformHooks.CallStaticJavaUnityMethod("com.parse.AVPushUnityHelper", "setApplicationPaused", new object[] { paused });
      }
    }

    private void Initialize() {
      if (!isInitialized) {
        isInitialized = true;
        // Keep this gameObject around, even when the scene changes.
        GameObject.DontDestroyOnLoad(gameObject);

        AVClient.Initialize(applicationID, dotnetKey);

        // Kick off the dispatcher.
        StartCoroutine(PlatformHooks.RunDispatcher());
      }
    }

    #region Android Callbacks

    /// <summary>
    /// The callback that will be called from the Android Java land via <c>UnityPlayer.UnitySendMessage(string)</c>
    /// when the device receive a push notificaiton.
    /// </summary>
    /// <param name="pushPayloadString">the push payload as string</param>
    internal void OnPushNotificationReceived(string pushPayloadString) {
      Initialize();

      AVPush.parsePushNotificationReceived.Invoke(AVInstallation.CurrentInstallation, new AVPushNotificationEventArgs(pushPayloadString));
    }

    /// <summary>
    /// The callback that will be called from the Android Java land via <c>UnityPlayer.UnitySendMessage(string)</c>
    /// when the device receive a GCM registration id.
    /// </summary>
    /// <param name="registrationId">the GCM registration id</param>
    internal void OnGcmRegistrationReceived(string registrationId) {
      Initialize();

      var installation = AVInstallation.CurrentInstallation;
      installation.DeviceToken = registrationId;
      // Set `pushType` via internal `Set` method since we want to skip mutability check.
      installation.Set("pushType", "gcm");

      // We can't really wait for this or else we'll block the thread.
      // We can only hope this operation will finish.
      installation.SaveAsync();
    }

    #endregion
  }
}
