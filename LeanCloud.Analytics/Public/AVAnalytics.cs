// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This Source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this Source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

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

        /// <summary>
        /// 本次对话的统计数据
        /// </summary>
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

        /// <summary>
        /// 本地对话的 Id，由本地生成，云端只做统计标识
        /// </summary>
        public string SessionId { get; internal set; }


        /// <summary>
        /// 云端是否打开了统计功能
        /// </summary>
        public bool Enable { get; internal set; }

        /// <summary>
        /// 统计数据发送的策略
        /// 1. 启动发送
        /// 6. 按照默认 30 次，批量发送
        /// 7. 按照最小时间间隔发送
        /// </summary>
        public int Policy { get; internal set; }

        /// <summary>
        /// 自定义云端参数
        /// </summary>
        public IDictionary<string, object> CloudParameters { get; internal set; }

        internal IAVAnalyticsDevice deviceHook;

        IList<AVAnalyticsEvent> @event;
        AVAnalyticsLaunch launch;
        AVAnalyticsTerminate terminate;

        /// <summary>
        /// 开启统计功能
        /// </summary>
        /// <param name="device">客户端参数，例如硬件，网络，版本，渠道等信息</param>
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
                Current = current;
                return current.Enable;
            });
        }

        /// <summary>
        /// 记录本地应用打开来自于用户主动打开
        /// </summary>
        public void TrackAppOpened()
        {
            this.TrackEvent("!AV!PushOpen", "!AV!PushOpen", null);
        }

        /// <summary>
        /// 记录本次应用打开来自于推送
        /// </summary>
        /// <param name="pushHash"></param>
        public void TrackAppOpenedWithPush(IDictionary<string, object> pushHash = null)
        {
            this.TrackEvent("!AV!PushOpen", "!AV!PushOpen", pushHash);
        }


        /// <summary>
        /// 记录一个自定义事件被触发
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <returns>事件 Id</returns>
        public string TrackEvent(string name)
        {
            return this.TrackEvent(name, null, null);
        }
        /// <summary>
        /// 记录一个自定义事件
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="tag">事件标签</param>
        /// <param name="attributes">自定义参数字典</param>
        /// <returns>事件 Id</returns>
        public string TrackEvent(string name, string tag, IDictionary<string, object> attributes)
        {
            var newEventId = string.Format("event_{0}", Guid.NewGuid().ToString());
            var newEvent = new AVAnalyticsEvent(this.SessionId)
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


        /// <summary>
        /// 开始记录一个持久化的自定义事件
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="tag">事件标签</param>
        /// <param name="attributes">自定义参数字典</param>
        /// <returns>事件 Id</returns>
        public string BeginEvent(string name, string tag, IDictionary<string, object> attributes = null)
        {
            return this.TrackEvent(name, tag, attributes);
        }


        /// <summary>
        /// 结束记录一个持久化的自定义事件
        /// </summary>
        /// <param name="eventId">事件 Id</param>
        /// <param name="attributes">自定义参数字典</param>
        /// <remarks>End 传入的 attributes 会合并 Begin 传入的 attributes 的键值对，如果有 key 重复，以 End 传入的为准</remarks>
        public void EndEvent(string eventId, IDictionary<string, object> attributes = null)
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

        /// <summary>
        /// 记录一个页面的访问时长
        /// </summary>
        /// <param name="name">页面名称</param>
        /// <param name="duration">访问时长，毫秒</param>
        /// <returns>页面 Id</returns>
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

        /// <summary>
        /// 开始记录一个页面的访问
        /// </summary>
        /// <param name="name">页面名称</param>
        /// <returns>页面 Id</returns>
        public string BeginPage(string name)
        {
            return TrackPage(name, 0);
        }

        /// <summary>
        /// 结束记录一个页面的访问
        /// </summary>
        /// <param name="pageId">页面 Id</param>
        public void EndPage(string pageId)
        {
            var begunPage = this.terminate.activities.First(a => a.activityId == pageId);
            if (begunPage == null)
            {
                throw new ArgumentOutOfRangeException("can not find page with id is " + pageId);
            }
            begunPage.du = AVAnalytics.UnixTimestampFromDateTime(DateTime.Now) - begunPage.ts;
        }

        /// <summary>
        /// 将当前统计的数据发送给云端
        /// </summary>
        /// <returns></returns>
        internal Task SendAsync()
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

        /// <summary>
        /// 关闭当前收集
        /// </summary>
        public void CloseSession()
        {
            this.SendAsync();
        }

        internal void Reset()
        {
            this.SessionId = Guid.NewGuid().ToString();
            @event = new List<AVAnalyticsEvent>();
            launch = new AVAnalyticsLaunch(SessionId);
            terminate = new AVAnalyticsTerminate(SessionId);
        }

        internal IDictionary<string, object> ToJson()
        {

            var rtn = new Dictionary<string, object>();
            lock (mutex)
            {
                rtn.Add("device", this.makeDeviceJson());

                rtn.Add("events", this.makeEventsJson());
            }
            return rtn;
        }

        internal IDictionary<string, object> makeDeviceJson()
        {
            var rtn = new Dictionary<string, object>();
            rtn.Add("access", this.deviceHook.access);
            rtn.Add("app_version", this.deviceHook.app_version);
            rtn.Add("carrier", this.deviceHook.carrier);
            rtn.Add("channel", this.deviceHook.channel);
            rtn.Add("device_id", this.deviceHook.device_id);
            rtn.Add("device_model", this.deviceHook.device_model);
            rtn.Add("device_brand", this.deviceHook.device_brand);

            rtn.Add("language", this.deviceHook.language);
            rtn.Add("mc", this.deviceHook.mc);
            rtn.Add("os", this.deviceHook.os);
            rtn.Add("os_version", this.deviceHook.os_version);
            rtn.Add("resolution", this.deviceHook.resolution);
            rtn.Add("timezone", this.deviceHook.timezone);
            return rtn;
        }

        internal IList<IDictionary<string, object>> makeEventJson()
        {
            var eventList = new List<IDictionary<string, object>>();
            foreach (var e in this.@event)
            {
                eventList.Add(e.ToJson());
            }
            return eventList;
        }

        internal IDictionary<string, object> makeEventsJson()
        {
            var rtn = new Dictionary<string, object>();
            rtn.Add("event", this.makeEventJson());
            rtn.Add("launch", this.launch.ToJson());
            rtn.Add("terminate", this.terminate.ToJson());
            return rtn;
        }

        internal static long UnixTimestampFromDateTime(DateTime date)
        {
            long unixTimestamp = date.Ticks - new DateTime(1970, 1, 1).Ticks;
            unixTimestamp /= TimeSpan.TicksPerMillisecond;
            return unixTimestamp;
        }
    }

    /// <summary>
    /// 客户端统计参数，包含一些硬件，网络，操作系统的参数
    /// </summary>
    public interface IAVAnalyticsDevice
    {
        /// <summary>
        /// 网络接入环境，例如 4G 或者 WIFI 等
        /// </summary>
        string access { get; }
        /// <summary>
        /// 应用版本 (必填)
        /// </summary>
        string app_version { get; }
        /// <summary>
        /// 网络运营商，例如中国移动，中国联通
        /// </summary>
        string carrier { get; }

        /// <summary>
        /// 分发渠道 (必填，例如 91，豌豆荚，App Store)
        /// </summary>
        string channel { get; }

        /// <summary>
        /// 设备 Id (必填)，只要保证一个设备唯一即可，可以自己生成
        /// </summary>
        string device_id { get; }

        /// <summary>
        /// 设备型号，例如 iPhone6,2(必填)
        /// </summary>
        string device_model { get; }

        /// <summary>
        /// 设备生产厂商，例如华为，小米
        /// </summary>
        string device_brand { get; }

        /// <summary>
        /// 语言，例如,zh-CN
        /// </summary>
        string language { get; }

        /// <summary>
        /// 设备的网络 MAC 地址
        /// </summary>
        string mc { get; }

        /// <summary>
        /// 运行平台，例如 iOS, Android,Windows,UWP ...(必填)
        /// </summary>
        string os { get; }

        /// <summary>
        /// 操作系统版本(必填)
        /// </summary>
        string os_version { get; }

        /// <summary>
        /// 设备分辨率,例如"640 x 1136"
        /// </summary>
        string resolution { get; }

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
                { "SessionId",sessionId },
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
                { "activities",encodeActivities.ToList() },
                { "SessionId",sessionId },
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
                { "SessionId",sessionId },
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
