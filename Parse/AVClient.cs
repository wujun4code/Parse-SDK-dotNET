// Copyright (c) 2015-present, LeanCloud, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using LeanCloud.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud {
  /// <summary>
  /// AVClient contains static functions that handle global
  /// configuration for the LeanCloud library.
  /// </summary>
  public static partial class AVClient {
      public enum AVRegion {
          CN = 0,
          US = 1,
#if DEBUG
          STAGE = 2
#endif
      }
    internal const string DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";

    private static readonly object mutex = new object();
    private static readonly string[] assemblyNames = {
      "LeanCloud.Phone", "LeanCloud.WinRT", "LeanCloud.NetFx45", "LeanCloud.iOS", "LeanCloud.Android", "LeanCloud.Unity"
    };

    static AVClient() {
      Type platformHookType = GetAVType("PlatformHooks");
      if (platformHookType == null) {
        throw new InvalidOperationException("You must include a reference to a platform-specific LeanCloud library.");
      }
      platformHooks = Activator.CreateInstance(platformHookType) as IPlatformHooks;
      commandRunner = new AVCommandRunner(platformHooks.HttpClient);
      versionString = "net-" + platformHooks.SDKName + Version;
    }

    private static Type GetAVType(string name) {
      foreach (var assembly in assemblyNames) {
        Type type = Type.GetType(string.Format("LeanCloud.{0}, {1}", name, assembly));
        if (type != null) {
          return type;
        }
      }
      return null;
    }

    private static readonly IPlatformHooks platformHooks;
    internal static IPlatformHooks PlatformHooks { get { return platformHooks; } }

    private static readonly IAVCommandRunner commandRunner;
    internal static IAVCommandRunner AVCommandRunner { get { return commandRunner; } }
    private readonly static string APIAddressCN = "https://api.leancloud.cn";

    private readonly static string APIAddressUS = "https://us-api.leancloud.cn";
    internal static Uri HostName { get; set; }
    internal static string MasterKey { get; set; }
    internal static string ApplicationId { get; set; }
    internal static string ApplicationKey { get; set; }

    internal static Version Version {
      get {
        var assemblyName = new AssemblyName(typeof(AVClient).GetTypeInfo().Assembly.FullName);
        return assemblyName.Version;
      }
    }

    private static readonly string versionString;
    internal static string VersionString {
      get {
        return versionString;
      }
    }

    /// <summary>
    /// Authenticates this client as belonging to your application. This must be
    /// called before your application can use the LeanCloud library. The recommended
    /// way is to put a call to <c>AVFramework.Initialize</c> in your
    /// Application startup.
    /// </summary>
    /// <param name="applicationId">The Application ID provided in the LeanCloud dashboard.
    /// </param>
    /// <param name="applicationKey">The Application Key provided in the LeanCloud dashboard.
    /// </param>
    public static void Initialize(string applicationId, string applicationKey) {
        Initialize(applicationId,applicationKey,AVRegion.CN);
    }

    /// <summary>
    /// 初始化 AVClient，所有 AVOS Cloud 的请求都是通过这个类去实现，所以在使用 sdk 之前，必须显示地调用Initialize方法。
    /// </summary>
    /// <param name="applicationId">Application ID 可以从 AVOS Cloud 控制台中->设置 找到。</param>
    /// <param name="appKey">App Key 可以从 AVOS Cloud 控制台中->设置 找到。</param>
    /// <param name="region">目前仅支持CN和US，默认值是CN。</param>
    public static void Initialize(string applicationId,string applicationKey,AVRegion region) {
#if DEBUG
        string[] apiHostCollections = { APIAddressCN,APIAddressUS,"https://cn-stg1.leancloud.cn" };
#else 
            string[] apiHostCollections = { APIAddressCN, APIAddressUS };
#endif

        int regionIndex = (int)region;
        if (regionIndex < 0 || regionIndex > apiHostCollections.Length) {
            throw new Exception("所选的地区不在 LeanCloud 的支持范围之内，请查阅文档。");
        }
        string targetHost = apiHostCollections[regionIndex];
        Initialize(applicationId,applicationKey,targetHost);
    }
    internal static void Initialize(string applicationId,string applicationKey,string apiHost) {
        lock (mutex) {
            HostName = HostName ?? new Uri("https://api.leancloud.cn/");
            ApplicationId = applicationId;
            ApplicationKey = applicationKey;

            AVObject.RegisterSubclass<AVUser>();
            AVObject.RegisterSubclass<AVInstallation>();
            AVObject.RegisterSubclass<AVRole>();
            AVObject.RegisterSubclass<AVSession>();

            // Give platform-specific libraries a chance to do additional initialization.
            PlatformHooks.Initialize();
        }
    }

    internal static Guid? InstallationId {
      get {
        return AVCorePlugins.Instance.InstallationIdController.Get();
      }
      set {
        AVCorePlugins.Instance.InstallationIdController.Set(value);
      }
    }

    internal static bool IsContainerObject(object value) {
      return value is AVACL ||
          value is AVGeoPoint ||
          ConvertTo<IDictionary<string, object>>(value) is IDictionary<string, object> ||
          ConvertTo<IList<object>>(value) is IList<object>;
    }

    /// <summary>
    /// Performs a ConvertTo, but returns null if the object can't be
    /// converted to that type.
    /// </summary>
    internal static T As<T>(object value) where T : class {
      return ConvertTo<T>(value) as T;
    }

    /// <summary>
    /// Converts a value to the requested type -- coercing primitives to
    /// the desired type, wrapping lists and dictionaries appropriately,
    /// or else passing the object along to the caller unchanged.
    /// 
    /// This should be used on any containers that might be coming from a
    /// user to normalize the collection types. Collection types coming from
    /// JSON deserialization can be safely assumed to be lists or dictionaries of
    /// objects.
    /// </summary>
    internal static object ConvertTo<T>(object value) {
      if (value is T || value == null) {
        return value;
      }

      if (typeof(T).IsPrimitive()) {
        return (T)Convert.ChangeType(value, typeof(T));
      }

      if (typeof(T).IsConstructedGenericType()) {
        // Add lifting for nullables. Only supports conversions between primitives.
        if (typeof(T).IsNullable()) {
          var innerType = typeof(T).GetGenericTypeArguments()[0];
          if (innerType.IsPrimitive()) {
            return (T)Convert.ChangeType(value, innerType);
          }
        }
        Type listType = GetInterfaceType(value.GetType(), typeof(IList<>));
        if (listType != null &&
            typeof(T).GetGenericTypeDefinition() == typeof(IList<>)) {
          var wrapperType = typeof(FlexibleListWrapper<,>).MakeGenericType(typeof(T).GetGenericTypeArguments()[0],
              listType.GetGenericTypeArguments()[0]);
          return Activator.CreateInstance(wrapperType, value);
        }
        Type dictType = GetInterfaceType(value.GetType(), typeof(IDictionary<,>));
        if (dictType != null &&
            typeof(T).GetGenericTypeDefinition() == typeof(IDictionary<,>)) {
          var wrapperType = typeof(FlexibleDictionaryWrapper<,>).MakeGenericType(typeof(T).GetGenericTypeArguments()[1],
              dictType.GetGenericTypeArguments()[1]);
          return Activator.CreateInstance(wrapperType, value);
        }
      }

      return value;
    }

    /// <summary>
    /// Holds a dictionary that maps a cache of interface types for related concrete types.
    /// The lookup is slow the first time for each type because it has to enumerate all interface
    /// on the object type, but made fast by the cache.
    /// 
    /// The map is:
    ///    (object type, generic interface type) => constructed generic type
    /// </summary>
    private static readonly Dictionary<Tuple<Type, Type>, Type> interfaceLookupCache =
        new Dictionary<Tuple<Type, Type>, Type>();
    private static Type GetInterfaceType(Type objType, Type genericInterfaceType) {
      // Side note: It so sucks to have to do this. What a piece of crap bit of code
      // Unfortunately, .NET doesn't provide any of the right hooks to do this for you
      // *sigh*
      if (genericInterfaceType.IsConstructedGenericType()) {
        genericInterfaceType = genericInterfaceType.GetGenericTypeDefinition();
      }
      var cacheKey = new Tuple<Type, Type>(objType, genericInterfaceType);
      if (interfaceLookupCache.ContainsKey(cacheKey)) {
        return interfaceLookupCache[cacheKey];
      }
      foreach (var type in objType.GetInterfaces()) {
        if (type.IsConstructedGenericType() &&
            type.GetGenericTypeDefinition() == genericInterfaceType) {
          return interfaceLookupCache[cacheKey] = type;
        }
      }
      return null;
    }

    internal static string BuildQueryString(IDictionary<string, object> parameters) {
      return string.Join("&", (from pair in parameters
                               let valueString = pair.Value as string
                               select string.Format("{0}={1}",
                                 Uri.EscapeDataString(pair.Key),
                                 Uri.EscapeDataString(string.IsNullOrEmpty(valueString) ?
                                    Json.Encode(pair.Value) : valueString)))
                                 .ToArray());
    }

    internal static IDictionary<string, string> DecodeQueryString(string queryString) {
      var dict = new Dictionary<string, string>();
      foreach (var pair in queryString.Split('&')) {
        var parts = pair.Split(new char[] { '=' }, 2);
        dict[parts[0]] = parts.Length == 2 ? Uri.UnescapeDataString(parts[1].Replace("+", " ")) : null;
      }
      return dict;
    }

    internal static IDictionary<string, object> DeserializeJsonString(string jsonData) {
      return Json.Parse(jsonData) as IDictionary<string, object>;
    }

    internal static string SerializeJsonString(IDictionary<string, object> jsonData) {
      return Json.Encode(jsonData);
    }

    internal static IDictionary<string, object> ApplicationSettings {
      get {
        return PlatformHooks.ApplicationSettings;
      }
    }

    /// <summary>
    /// Convenience alias for RequestAsync that takes a string instead of a Uri.
    /// </summary>
    internal static Task<Tuple<HttpStatusCode,IDictionary<string,object>>> RequestAsync(string method,string relativeUri,string sessionToken,IDictionary<string,object> data,CancellationToken cancellationToken) {
        return AVClient.RequestAsync(method,new Uri(relativeUri,UriKind.Relative),sessionToken,data,cancellationToken);
    }
    internal static Task<Tuple<HttpStatusCode,string>> RequestAsync(Uri uri,string method,IList<KeyValuePair<string,string>> headers,Stream data,string contentType,CancellationToken cancellationToken) {
        if (method == null) {
            if (data != null) {
                method = "POST";
            } else {
                method = "GET";
            }
        }

        HttpRequest request =new HttpRequest(){
            Data = data,
            Headers = headers,
            Method = method,
            Uri = uri
        };

       return AVClient.PlatformHooks.HttpClient.ExecuteAsync(request,null,null,CancellationToken.None);
    }
    internal static Task<Tuple<HttpStatusCode,IDictionary<string,object>>> RequestAsync(string method,Uri relativeUri,string sessionToken,IDictionary<string,object> data,CancellationToken cancellationToken) {

        var command = new AVCommand(relativeUri.ToString(),
            method :method,
            sessionToken :sessionToken,
            data :null);
   
        return AVClient.AVCommandRunner.RunCommandAsync(command,cancellationToken :cancellationToken);
    }
    internal static Tuple<HttpStatusCode,IDictionary<string,object>> ReponseResolve(Tuple<HttpStatusCode,string> response,CancellationToken cancellationToken) {
        Tuple<HttpStatusCode,string> result = response;
        string item2 = result.Item2;
        HttpStatusCode code = result.Item1;
        if (item2 == null) {
            cancellationToken.ThrowIfCancellationRequested();
            return new Tuple<HttpStatusCode,IDictionary<string,object>>(code,null);
        }
        IDictionary<string,object> strs = null;
        try {
            strs = ( !item2.StartsWith("[") ? AVClient.DeserializeJsonString(item2) : new Dictionary<string,object>()
					{
						{ "results", Json.Parse(item2) }
					} );
        } catch (Exception exception) {
            throw new AVException(AVException.ErrorCode.OtherCause,"Invalid response from server",exception);
        }
        var codeValue = (int)code;
        if (codeValue > 203 || codeValue < 200) {
            throw new AVException((AVException.ErrorCode)( (int)( ( strs.ContainsKey("code") ? (long)strs["code"] : (long)-1 ) ) ),( strs.ContainsKey("error") ? strs["error"] as string : item2 ),null);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return new Tuple<HttpStatusCode,IDictionary<string,object>>(code,strs);
    }
		internal static bool IsSuccessStatusCode(HttpStatusCode responseStatus){
			var codeValue = (int)responseStatus;
			return (codeValue > 199) && (codeValue < 204);
		}
  }
}
