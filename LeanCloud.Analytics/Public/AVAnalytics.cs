// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using LeanCloud.Storage.Internal;
using LeanCloud.Analytics.Internal;
using LeanCloud.Core.Internal;
using System.Linq;
using System.Linq.Expressions;

namespace LeanCloud
{
    /// <summary>
    /// Provides an interface to LeanCloud's logging and analytics backend.
    ///
    /// Methods will return immediately and cache requests (along with timestamps)
    /// to be handled in the background.
    /// </summary>
    public partial class AVAnalytics
    {
        internal static IAVAnalyticsController AnalyticsController
        {
            get
            {
                return AVAnalyticsPlugins.Instance.AnalyticsController;
            }
        }

        internal static IAVCurrentUserController CurrentUserController
        {
            get
            {
                return AVAnalyticsPlugins.Instance.CorePlugins.CurrentUserController;
            }
        }

        private static AVAnalytics _current;
        public static AVAnalytics Current
        {
            get
            {
                return _current;
            }
            internal set
            {
                _current = value;
            }
        }

        internal readonly object mutex = new object();

        public string sessionId;
        public bool Enable { get; internal set; }

        public int Policy { get; internal set; }

        public IDictionary<string, object> CloudParameters { get; internal set; }

        internal IAVAnalyticsDevice deviceHook;

        IList<AVAnalyticsEvent> @event;
        AVAnalyticsLaunch launch;
        AVAnalyticsTerminate terminate;

        /// <summary>
        /// 初始化统计功能，可能会因为服务端关闭而终止，因此确保控制台里面打开了统计功能
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static Task<bool> InitAsync(IAVAnalyticsDevice device)
        {
            if (device == null)
                throw new NotImplementedException("InitAsync need an IAVAnalyticsDevice implementment");

            return CurrentUserController.GetCurrentSessionTokenAsync(CancellationToken.None).OnSuccess(t =>
            {
                return AVAnalytics.AnalyticsController.GetPolicyAsync(t.Result, CancellationToken.None);
            }).Unwrap().OnSuccess(s =>
            {
                var settings = s.Result;
                var current = new AVAnalytics();
                current.CloudParameters = settings["parameters"] as IDictionary<string, object>;
                current.Enable = bool.Parse(settings["enable"].ToString());
                current.Policy = int.Parse(settings["policy"].ToString());
                current.deviceHook = device;
                current.Reset();
                return current.Enable;
            });
        }

        public void TrackAppOpened()
        {
            this.TrackEvent("!AV!PushOpen", "!AV!PushOpen", null);
        }

        public void TrackAppOpenedWithPush(IDictionary<string,object> pushHash = null)
        {
            this.TrackEvent("!AV!PushOpen", "!AV!PushOpen", pushHash);
        }

        public string TrackEvent(string name, string tag, IDictionary<string, object> attributes)
        {
            var newEventId = string.Format("event_{0}", Guid.NewGuid().ToString());
            var newEvent = new AVAnalyticsEvent(this.sessionId)
            {
                eventId = newEventId,
                attributes = attributes,
                name = name,
                tag = tag,
                ts = AVAnalytics.UnixTimestampFromDateTime(DateTime.Now),
                du = 0,
            };
            this.@event.Add(newEvent);
            return newEventId;
        }

        public string BeginEvent(string name, string tag, IDictionary<string, object> attributes)
        {
            return this.TrackEvent(name, tag, attributes);
        }

        public void EndEvent(string eventId, IDictionary<string, object> attributes)
        {
            var begunEvent = this.@event.First(e => e.eventId == eventId);

            if (begunEvent == null)
            {
                throw new ArgumentOutOfRangeException("can not find event whick id is " + eventId);
            }
            begunEvent.du = AVAnalytics.UnixTimestampFromDateTime(DateTime.Now) - begunEvent.ts;
            if (attributes != null)
            {
                if (begunEvent.attributes == null) begunEvent.attributes = new Dictionary<string, object>();
                foreach (var kv in attributes)
                {
                    begunEvent.attributes[kv.Key] = kv.Value;
                }
            }
        }

        public string TrackPage(string name, long duration)
        {
            var newActivityId = string.Format("activity_{0}", Guid.NewGuid().ToString());
            var newActivity = new AVAnalyticsActivity()
            {
                activityId = newActivityId,
                du = duration,
                name = name,
                ts = AVAnalytics.UnixTimestampFromDateTime(DateTime.Now)
            };
            this.terminate.activities.Add(newActivity);
            return newActivityId;
        }

        public string BeginPage(string name)
        {
            return TrackPage(name, 0);
        }

        public void EndPage(string pageId)
        {
            var begunPage = this.terminate.activities.First(a => a.activityId == pageId);
            if (begunPage == null)
            {
                throw new ArgumentOutOfRangeException("can not find page whick id is " + pageId);
            }
            begunPage.du = AVAnalytics.UnixTimestampFromDateTime(DateTime.Now) - begunPage.ts;
        }

        /// <summary>
        /// 发送本地统计数据
        /// </summary>
        /// <returns></returns>
        public Task SendAsync()
        {
            if (!this.Enable) return Task.FromResult(false);
            var analyticsData = this.ToJson();
            return CurrentUserController.GetCurrentSessionTokenAsync(CancellationToken.None).OnSuccess(t =>
            {
                return AVAnalytics.AnalyticsController.SendAsync(analyticsData, t.Result, CancellationToken.None);
            }).Unwrap();
        }

        internal void StartSession()
        {

        }

        internal void Reset()
        {
            this.sessionId = Guid.NewGuid().ToString();
            @event = new List<AVAnalyticsEvent>();
            launch = new AVAnalyticsLaunch(sessionId);
            terminate = new AVAnalyticsTerminate(sessionId);
        }

        internal IDictionary<string, object> ToJson()
        {

            var rtn = new Dictionary<string, object>();
            lock (mutex)
            {
            }
            return rtn;
        }

        internal static long UnixTimestampFromDateTime(DateTime date)
        {
            long unixTimestamp = date.Ticks - new DateTime(1970, 1, 1).Ticks;
            unixTimestamp /= TimeSpan.TicksPerMillisecond;
            return unixTimestamp;
        }
    }
    

    public interface IAVAnalyticsDevice
    {
        /// <summary>
        /// 联网方式
        /// </summary>
        string access { get; }
        /// <summary>
        /// 应用版本 (必填)
        /// </summary>
        string app_version { get; }
        /// <summary>
        /// 运营商
        /// </summary>
        string carrier { get; }

        /// <summary>
        /// 发布渠道 (必填)
        /// </summary>
        string channel { get; }

        /// <summary>
        /// 设备id (必填) 这个要保证不同机器不重复，同一个机器不要变化
        /// </summary>
        string device_id { get; }

        /// <summary>
        /// 设备型号 (必填)
        /// </summary>
        string device_model { get; }

        /// <summary>
        /// 应用名称
        /// </summary>
        string display_name { get; }

        /// <summary>
        /// 推送的Installation表的object id，（如果有的话）
        /// </summary>
        string iid { get; }

        /// <summary>
        /// 越狱
        /// </summary>
        bool is_jailbroken { get; }

        /// <summary>
        /// 语言
        /// </summary>
        string language { get; }

        /// <summary>
        /// mac 地址
        /// </summary>
        string mc { get; }

        /// <summary>
        /// 平台（iOS, Android,Windows,UWP ... ）(必填)
        /// </summary>
        string os { get; }

        /// <summary>
        /// 系统版本 (必填)
        /// </summary>
        string os_version { get; }

        /// <summary>
        /// 应用包名
        /// </summary>
        string package_name { get; }

        /// <summary>
        /// 设备分辨率,例如："640 x 1136"
        /// </summary>
        string resolution { get; }

        /// <summary>
        /// LeanCloud SDK版本 (必填)
        /// </summary>
        string sdk_version { get; }

        /// <summary>
        /// 应用 Build 版本号
        /// </summary>
        string sv { get; }

        /// <summary>
        /// 时区
        /// </summary>
        string timezone { get; }
    }

    internal class AVAnalyticsLaunch
    {
        public AVAnalyticsLaunch(string _sessionId)
        {
            this.sessionId = _sessionId;
        }
        public long date;
        public string sessionId;

        public IDictionary<string, object> ToJson()
        {
            return new Dictionary<string, object>()
            {
                { "date",date },
                { "sessionId",sessionId },
            };
        }
    }
    internal class AVAnalyticsTerminate
    {
        public AVAnalyticsTerminate(string _sessionId)
        {
            this.sessionId = _sessionId;
            this.activities = new List<AVAnalyticsActivity>();
        }
        public long duration;
        public IList<AVAnalyticsActivity> activities;
        public string sessionId;
        public IDictionary<string, object> ToJson()
        {
            var encodeActivities = from activity in activities
                                   select activity.ToJson();
            return new Dictionary<string, object>()
            {
                { "duration",duration },
                { "activities",Json.Encode(activities.ToList()) },
                { "sessionId",sessionId },
            };
        }
    }

    internal class AVAnalyticsActivity
    {
        public AVAnalyticsActivity()
        {

        }
        public string activityId;
        public long du;
        public string name;
        public long ts;

        public IDictionary<string, object> ToJson()
        {
            return new Dictionary<string, object>()
            {
                { "activityId",activityId },
                { "du",du },
                { "name",name },
                { "ts",ts },
            };
        }
    }

    internal class AVAnalyticsEvent
    {
        public AVAnalyticsEvent(string _sessionId)
        {
            this.sessionId = _sessionId;
        }
        public IDictionary<string, object> ToJson()
        {
            return new Dictionary<string, object>()
            {
                { "eventId",eventId },
                { "du",du },
                { "name",name },
                { "tag",tag },
                { "ts",ts },
                { "attributes",attributes },
                { "sessionId",sessionId },
            };
        }
        public string eventId;
        public long du;
        public string name;
        public string tag;
        public long ts;
        public IDictionary<string, object> attributes;
        public string sessionId;
    }
}
