// Copyright (c) 2015-present, LeanCloud, LLC.  All rights reserved.  This Source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this Source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using LeanCloud;
using LeanCloud.Storage.Internal;
using LeanCloud.Core.Internal;
using LeanCloud.Push.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud {

  /// <summary>
  ///  Represents this app installed on this device. Use this class to track information you want
  ///  to sample from (i.e. if you update a field on app launch, you can issue a query to see
  ///  the number of devices which were active in the last N hours).
  /// </summary>
  [AVClassName("_Installation")]
  public partial class AVInstallation : AVObject {
    private static readonly HashSet<string> readOnlyKeys = new HashSet<string> {
      "deviceType", "deviceUris", "installationId", "timeZone", "localeIdentifier", "parseVersion", "appName", "appIdentifier", "appVersion", "pushType"
    };

    internal static IAVCurrentInstallationController CurrentInstallationController {
      get {
        return AVPushPlugins.Instance.CurrentInstallationController;
      }
    }

    internal static IDeviceInfoController DeviceInfoController {
      get {
        return AVPushPlugins.Instance.DeviceInfoController;
      }
    }

    /// <summary>
    /// Constructs a new AVInstallation. Generally, you should not need to construct
    /// ParseInstallations yourself. Instead use <see cref="CurrentInstallation"/>.
    /// </summary>
    public AVInstallation()
      : base() {
    }

    /// <summary>
    /// Gets the AVInstallation representing this app on this device.
    /// </summary>
    public static AVInstallation CurrentInstallation {
      get {
        var task = CurrentInstallationController.GetAsync(CancellationToken.None);
        // TODO (hallucinogen): this will absolutely break on Unity, but how should we resolve this?
        task.Wait();
        return task.Result;
      }
    }

    internal static void ClearInMemoryInstallation() {
      CurrentInstallationController.ClearFromMemory();
    }

    /// <summary>
    /// Constructs a <see cref="AVQuery{AVInstallation}"/> for ParseInstallations.
    /// </summary>
    /// <remarks>
    /// Only the following types of queries are allowed for installations:
    ///
    /// <code>
    /// query.GetAsync(objectId)
    /// query.WhereEqualTo(key, value)
    /// query.WhereMatchesKeyInQuery&lt;TOther&gt;(key, keyInQuery, otherQuery)
    /// </code>
    ///
    /// You can add additional query conditions, but one of the above must appear as a top-level <c>AND</c>
    /// clause in the query.
    /// </remarks>
    public static AVQuery<AVInstallation> Query {
      get {
        return new AVQuery<AVInstallation>();
      }
    }

    /// <summary>
    /// A GUID that uniquely names this app installed on this device.
    /// </summary>
    [AVFieldName("installationId")]
    public Guid InstallationId {
      get {
        string installationIdString = GetProperty<string>("InstallationId");
        Guid? installationId = null;
        try {
          installationId = new Guid(installationIdString);
        } catch (Exception) {
          // Do nothing.
        }

        return installationId.Value;
      }
      internal set {
        Guid installationId = value;
        SetProperty<string>(installationId.ToString(), "InstallationId");
      }
    }

    /// <summary>
    /// The runtime target of this installation object.
    /// </summary>
    [AVFieldName("deviceType")]
    public string DeviceType {
      get { return GetProperty<string>("DeviceType"); }
      internal set { SetProperty<string>(value, "DeviceType"); }
    }

    /// <summary>
    /// The user-friendly display name of this application.
    /// </summary>
    [AVFieldName("appName")]
    public string AppName {
      get { return GetProperty<string>("AppName"); }
      internal set { SetProperty<string>(value, "AppName"); }
    }

    /// <summary>
    /// A version string consisting of Major.Minor.Build.Revision.
    /// </summary>
    [AVFieldName("appVersion")]
    public string AppVersion {
      get { return GetProperty<string>("AppVersion"); }
      internal set { SetProperty<string>(value, "AppVersion"); }
    }

    /// <summary>
    /// The system-dependent unique identifier of this installation. This identifier should be
    /// sufficient to distinctly name an app on stores which may allow multiple apps with the
    /// same display name.
    /// </summary>
    [AVFieldName("appIdentifier")]
    public string AppIdentifier {
      get { return GetProperty<string>("AppIdentifier"); }
      internal set { SetProperty<string>(value, "AppIdentifier"); }
    }

    /// <summary>
    /// The time zone in which this device resides. This string is in the tz database format
    /// LeanCloud uses for local-time pushes. Due to platform restrictions, the mapping is less
    /// granular on Windows than it may be on other systems. E.g. The zones
    /// America/Vancouver America/Dawson America/Whitehorse, America/Tijuana, PST8PDT, and
    /// America/Los_Angeles are all reported as America/Los_Angeles.
    /// </summary>
    [AVFieldName("timeZone")]
    public string TimeZone {
      get { return GetProperty<string>("TimeZone"); }
      private set { SetProperty<string>(value, "TimeZone"); }
    }

    /// <summary>
    /// The users locale. This field gets automatically populated by the SDK.
    /// Can be null (LeanCloud Push uses default language in this case).
    /// </summary>
    [AVFieldName("localeIdentifier")]
    public string LocaleIdentifier {
      get { return GetProperty<string>("LocaleIdentifier"); }
      private set { SetProperty<string>(value, "LocaleIdentifier"); }
    }

    /// <summary>
    /// Gets the locale identifier in the format: [language code]-[COUNTRY CODE].
    /// </summary>
    /// <returns>The locale identifier in the format: [language code]-[COUNTRY CODE].</returns>
    private string GetLocaleIdentifier() {
      String languageCode = null;
      String countryCode = null;
      if (CultureInfo.CurrentCulture != null) {
        languageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
      }
      if (RegionInfo.CurrentRegion != null) {
        countryCode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
      }
      if (string.IsNullOrEmpty(countryCode)) {
        return languageCode;
      } else {
        return string.Format("{0}-{1}", languageCode, countryCode);
      }
    }

    /// <summary>
    /// The version of the LeanCloud SDK used to build this application.
    /// </summary>
    [AVFieldName("parseVersion")]
    public Version AVVersion {
      get {
        var versionString = GetProperty<string>("AVVersion");
        Version version = null;
        try {
          version = new Version(versionString);
        } catch (Exception) {
          // Do nothing.
        }

        return version;
      }
      private set {
        Version version = value;
        SetProperty<string>(version.ToString(), "AVVersion");
      }
    }

    /// <summary>
    /// A sequence of arbitrary strings which are used to identify this installation for push notifications.
    /// By convention, the empty string is known as the "Broadcast" channel.
    /// </summary>
    [AVFieldName("channels")]
    public IList<string> Channels {
      get { return GetProperty<IList<string>>("Channels"); }
      set { SetProperty(value, "Channels"); }
    }

    protected override bool IsKeyMutable(string key) {
      return !readOnlyKeys.Contains(key);
    }

    protected override Task SaveAsync(Task toAwait, CancellationToken cancellationToken) {
      Task platformHookTask = null;
      if (CurrentInstallationController.IsCurrent(this)) {
        var configuration = AVClient.CurrentConfiguration;

        // 'this' is required in order for the extension method to be used.
        this.SetIfDifferent("deviceType", DeviceInfoController.DeviceType);
        this.SetIfDifferent("timeZone", DeviceInfoController.DeviceTimeZone);
        this.SetIfDifferent("localeIdentifier", GetLocaleIdentifier());
        this.SetIfDifferent("parseVersion", GetParseVersion().ToString());
        this.SetIfDifferent("appVersion", configuration.VersionInfo.BuildVersion ?? DeviceInfoController.AppBuildVersion);
        this.SetIfDifferent("appIdentifier", DeviceInfoController.AppIdentifier);
        this.SetIfDifferent("appName", DeviceInfoController.AppName);

        platformHookTask = DeviceInfoController.ExecuteParseInstallationSaveHookAsync(this);
      }

      return platformHookTask.Safe().OnSuccess(_ => {
        return base.SaveAsync(toAwait, cancellationToken);
      }).Unwrap().OnSuccess(_ => {
        if (CurrentInstallationController.IsCurrent(this)) {
          return Task.FromResult(0);
        }
        return CurrentInstallationController.SetAsync(this, cancellationToken);
      }).Unwrap();
    }

    private Version GetParseVersion() {
      return new AssemblyName(typeof(AVInstallation).GetTypeInfo().Assembly.FullName).Version;
    }

    /// <summary>
    /// This mapping of Windows names to a standard everyone else uses is maintained
    /// by the Unicode consortium, which makes this officially the first helpful
    /// interaction between Unicode and Microsoft.
    /// Unfortunately this is a little lossy in that we only store the first mapping in each zone because
    /// Microsoft does not give us more granular location information.
    /// Built from http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/zone_tzid.html
    /// </summary>
    internal static readonly Dictionary<string, string> TimeZoneNameMap = new Dictionary<string, string>() {
      {"Dateline Standard Time", "Etc/GMT+12"},
      {"UTC-11", "Etc/GMT+11"},
      {"Hawaiian Standard Time", "Pacific/Honolulu"},
      {"Alaskan Standard Time", "America/Anchorage"},
      {"Pacific Standard Time (Mexico)", "America/Santa_Isabel"},
      {"Pacific Standard Time", "America/Los_Angeles"},
      {"US Mountain Standard Time", "America/Phoenix"},
      {"Mountain Standard Time (Mexico)", "America/Chihuahua"},
      {"Mountain Standard Time", "America/Denver"},
      {"Central America Standard Time", "America/Guatemala"},
      {"Central Standard Time", "America/Chicago"},
      {"Central Standard Time (Mexico)", "America/Mexico_City"},
      {"Canada Central Standard Time", "America/Regina"},
      {"SA Pacific Standard Time", "America/Bogota"},
      {"Eastern Standard Time", "America/New_York"},
      {"US Eastern Standard Time", "America/Indianapolis"},
      {"Venezuela Standard Time", "America/Caracas"},
      {"Paraguay Standard Time", "America/Asuncion"},
      {"Atlantic Standard Time", "America/Halifax"},
      {"Central Brazilian Standard Time", "America/Cuiaba"},
      {"SA Western Standard Time", "America/La_Paz"},
      {"Pacific SA Standard Time", "America/Santiago"},
      {"Newfoundland Standard Time", "America/St_Johns"},
      {"E. South America Standard Time", "America/Sao_Paulo"},
      {"Argentina Standard Time", "America/Buenos_Aires"},
      {"SA Eastern Standard Time", "America/Cayenne"},
      {"Greenland Standard Time", "America/Godthab"},
      {"Montevideo Standard Time", "America/Montevideo"},
      {"Bahia Standard Time", "America/Bahia"},
      {"UTC-02", "Etc/GMT+2"},
      {"Azores Standard Time", "Atlantic/Azores"},
      {"Cape Verde Standard Time", "Atlantic/Cape_Verde"},
      {"Morocco Standard Time", "Africa/Casablanca"},
      {"UTC", "Etc/GMT"},
      {"GMT Standard Time", "Europe/London"},
      {"Greenwich Standard Time", "Atlantic/Reykjavik"},
      {"W. Europe Standard Time", "Europe/Berlin"},
      {"Central Europe Standard Time", "Europe/Budapest"},
      {"Romance Standard Time", "Europe/Paris"},
      {"Central European Standard Time", "Europe/Warsaw"},
      {"W. Central Africa Standard Time", "Africa/Lagos"},
      {"Namibia Standard Time", "Africa/Windhoek"},
      {"GTB Standard Time", "Europe/Bucharest"},
      {"Middle East Standard Time", "Asia/Beirut"},
      {"Egypt Standard Time", "Africa/Cairo"},
      {"Syria Standard Time", "Asia/Damascus"},
      {"E. Europe Standard Time", "Asia/Nicosia"},
      {"South Africa Standard Time", "Africa/Johannesburg"},
      {"FLE Standard Time", "Europe/Kiev"},
      {"Turkey Standard Time", "Europe/Istanbul"},
      {"Israel Standard Time", "Asia/Jerusalem"},
      {"Jordan Standard Time", "Asia/Amman"},
      {"Arabic Standard Time", "Asia/Baghdad"},
      {"Kaliningrad Standard Time", "Europe/Kaliningrad"},
      {"Arab Standard Time", "Asia/Riyadh"},
      {"E. Africa Standard Time", "Africa/Nairobi"},
      {"Iran Standard Time", "Asia/Tehran"},
      {"Arabian Standard Time", "Asia/Dubai"},
      {"Azerbaijan Standard Time", "Asia/Baku"},
      {"Russian Standard Time", "Europe/Moscow"},
      {"Mauritius Standard Time", "Indian/Mauritius"},
      {"Georgian Standard Time", "Asia/Tbilisi"},
      {"Caucasus Standard Time", "Asia/Yerevan"},
      {"Afghanistan Standard Time", "Asia/Kabul"},
      {"Pakistan Standard Time", "Asia/Karachi"},
      {"West Asia Standard Time", "Asia/Tashkent"},
      {"India Standard Time", "Asia/Calcutta"},
      {"Sri Lanka Standard Time", "Asia/Colombo"},
      {"Nepal Standard Time", "Asia/Katmandu"},
      {"Central Asia Standard Time", "Asia/Almaty"},
      {"Bangladesh Standard Time", "Asia/Dhaka"},
      {"Ekaterinburg Standard Time", "Asia/Yekaterinburg"},
      {"Myanmar Standard Time", "Asia/Rangoon"},
      {"SE Asia Standard Time", "Asia/Bangkok"},
      {"N. Central Asia Standard Time", "Asia/Novosibirsk"},
      {"China Standard Time", "Asia/Shanghai"},
      {"North Asia Standard Time", "Asia/Krasnoyarsk"},
      {"Singapore Standard Time", "Asia/Singapore"},
      {"W. Australia Standard Time", "Australia/Perth"},
      {"Taipei Standard Time", "Asia/Taipei"},
      {"Ulaanbaatar Standard Time", "Asia/Ulaanbaatar"},
      {"North Asia East Standard Time", "Asia/Irkutsk"},
      {"Tokyo Standard Time", "Asia/Tokyo"},
      {"Korea Standard Time", "Asia/Seoul"},
      {"Cen. Australia Standard Time", "Australia/Adelaide"},
      {"AUS Central Standard Time", "Australia/Darwin"},
      {"E. Australia Standard Time", "Australia/Brisbane"},
      {"AUS Eastern Standard Time", "Australia/Sydney"},
      {"West Pacific Standard Time", "Pacific/Port_Moresby"},
      {"Tasmania Standard Time", "Australia/Hobart"},
      {"Yakutsk Standard Time", "Asia/Yakutsk"},
      {"Central Pacific Standard Time", "Pacific/Guadalcanal"},
      {"Vladivostok Standard Time", "Asia/Vladivostok"},
      {"New Zealand Standard Time", "Pacific/Auckland"},
      {"UTC+12", "Etc/GMT-12"},
      {"Fiji Standard Time", "Pacific/Fiji"},
      {"Magadan Standard Time", "Asia/Magadan"},
      {"Tonga Standard Time", "Pacific/Tongatapu"},
      {"Samoa Standard Time", "Pacific/Apia"}
    };

    /// <summary>
    /// This is a mapping of odd TimeZone offsets to their respective IANA codes across the world.
    /// This list was compiled from painstakingly pouring over the information available at
    /// https://en.wikipedia.org/wiki/List_of_tz_database_time_zones.
    /// </summary>
    internal static readonly Dictionary<TimeSpan, String> TimeZoneOffsetMap = new Dictionary<TimeSpan, string>() {
      { new TimeSpan(12, 45, 0), "Pacific/Chatham" },
      { new TimeSpan(10, 30, 0), "Australia/Lord_Howe" },
      { new TimeSpan(9, 30, 0),  "Australia/Adelaide" },
      { new TimeSpan(8, 45, 0), "Australia/Eucla" },
      { new TimeSpan(8, 30, 0), "Asia/Pyongyang" }, // LeanCloud in North Korea confirmed.
      { new TimeSpan(6, 30, 0), "Asia/Rangoon" },
      { new TimeSpan(5, 45, 0), "Asia/Kathmandu" },
      { new TimeSpan(5, 30, 0), "Asia/Colombo" },
      { new TimeSpan(4, 30, 0), "Asia/Kabul" },
      { new TimeSpan(3, 30, 0), "Asia/Tehran" },
      { new TimeSpan(-3, 30, 0), "America/St_Johns" },
      { new TimeSpan(-4, 30, 0), "America/Caracas" },
      { new TimeSpan(-9, 30, 0), "Pacific/Marquesas" },
    };
  }
}
