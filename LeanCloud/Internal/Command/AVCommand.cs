// Copyright (c) 2015-present, Parse, LLC  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LeanCloud.Internal {
  /// <summary>
  /// AVCommand is an <see cref="HttpRequest"/> with pre-populated
  /// headers.
  /// </summary>
  internal class AVCommand : HttpRequest {
    private const string revocableSessionTokenTrueValue = "1";

    public AVCommand(string relativeUri,
        string method,
        string sessionToken = null,
        IList<KeyValuePair<string, string>> headers = null,
        IDictionary<string, object> data = null) : this(relativeUri: relativeUri,
            method: method,
            sessionToken: sessionToken,
            headers: headers,
            stream: data != null ? new MemoryStream(UTF8Encoding.UTF8.GetBytes(Json.Encode(data))) : null,
            contentType: data != null ? "application/json" : null) {
    }

    public AVCommand(string relativeUri,
        string method,
        string sessionToken = null,
        IList<KeyValuePair<string, string>> headers = null,
        Stream stream = null,
        string contentType = null) {
      Uri = new Uri(AVClient.HostName, "/1.1" + relativeUri);
      Method = method;
      Data = stream;

      Headers = new List<KeyValuePair<string, string>> {
        new KeyValuePair<string, string>("X-LC-Id", AVClient.ApplicationId),
        new KeyValuePair<string, string>("X-LC-Client-Version", AVClient.VersionString),
        new KeyValuePair<string, string>("X-LC-Installation-Id", AVClient.InstallationId.ToString())
      };

      if (headers != null) {
        foreach (var header in headers) {
          Headers.Add(header);
        }
      }

      if (!string.IsNullOrEmpty(AVClient.PlatformHooks.AppBuildVersion)) {
        Headers.Add(new KeyValuePair<string, string>("X-LC-App-Build-Version", AVClient.PlatformHooks.AppBuildVersion));
      }
      if (!string.IsNullOrEmpty(AVClient.PlatformHooks.AppDisplayVersion)) {
        Headers.Add(new KeyValuePair<string, string>("X-LC-App-Display-Version", AVClient.PlatformHooks.AppDisplayVersion));
      }
      if (!string.IsNullOrEmpty(AVClient.PlatformHooks.OSVersion)) {
        Headers.Add(new KeyValuePair<string, string>("X-LC-OS-Version", AVClient.PlatformHooks.OSVersion));
      }
      if (!string.IsNullOrEmpty(AVClient.MasterKey)) {
        Headers.Add(new KeyValuePair<string, string>("X-LC-Key,matser", AVClient.MasterKey));
      } else {
        Headers.Add(new KeyValuePair<string, string>("X-LC-Key", AVClient.ApplicationKey));
      }
      if (!string.IsNullOrEmpty(sessionToken)) {
        Headers.Add(new KeyValuePair<string, string>("X-LC-Session", sessionToken));
      }
      if (!string.IsNullOrEmpty(contentType)) {
        Headers.Add(new KeyValuePair<string, string>("Content-Type", contentType));
      }
      if (AVUser.IsRevocableSessionEnabled) {
        Headers.Add(new KeyValuePair<string, string>("X-LC-Revocable-Session", revocableSessionTokenTrueValue));
      }
    }
  }
}
