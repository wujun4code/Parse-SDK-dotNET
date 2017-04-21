using Moq;
using NUnit.Framework;
using LeanCloud;
using LeanCloud.Analytics.Internal;
using LeanCloud.Core.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace ParseTest
{
    [TestFixture]
    public class AnalyticsTests
    {
        [SetUp]
        public void initApp()
        {
            string appId = ConfigurationManager.AppSettings["appId"];
            string appKey = ConfigurationManager.AppSettings["appKey"];
            AVClient.Initialize(appId, appKey);
        }

        [TearDown]
        public void TearDown()
        {
            AVAnalyticsPlugins.Instance.Reset();
        }

        [Test]
        public Task TestSendAnalyticsData()
        {
            return AVAnalytics.InitAsync(new PC()).ContinueWith(t =>
            {

                AVAnalytics.Current.TrackAppOpened();
                AVAnalytics.Current.TrackAppOpenedWithPush();
                var pageId = AVAnalytics.Current.BeginPage("LogInPage");
                AVAnalytics.Current.TrackEvent("LogIn_Button_Clicked");
                AVAnalytics.Current.TrackEvent("Gesture_Flick");

                var inputEventId = AVAnalytics.Current.BeginEvent("Input", "SignUpAction", new Dictionary<string, object>()
                {
                    { "age",27 },
                    { "gender","female"},
                });

                var orderEventId = AVAnalytics.Current.BeginEvent("Order", "OrderAction", new Dictionary<string, object>()
                {
                    { "clicked_ads","left" },//用户点击了左边栏的广告
                    { "scorll_down",true},//用户拉到了页面底部
                });
                AVAnalytics.Current.EndEvent(inputEventId);
                AVAnalytics.Current.EndPage(pageId);
                AVAnalytics.Current.CloseSession();
                return Task.FromResult(0);
            });
        }
    }

    public class PC : IAVAnalyticsDevice
    {
        public string access
        {
            get
            {
                return "WIFI";
            }
        }

        public string app_version
        {
            get
            {
                return "0.12.34";
            }
        }

        public string carrier
        {
            get
            {
                return "China Unicom";
            }
        }

        public string channel
        {
            get
            {
                return "Market Place";
            }
        }

        public string device_brand
        {
            get
            {
                return "ASUS";
            }
        }

        public string device_id
        {
            get
            {
                return "98DD09BDFDC24E359E0426219E9FA79A";
            }
        }

        public string device_model
        {
            get
            {
                return "Windows 10";
            }
        }

        public string iid
        {
            get
            {
                return null;
            }
        }

        public string language
        {
            get
            {
                return "zh-CN";
            }
        }

        public string mc
        {
            get
            {
                return "02:00:00:00:00:00";
            }
        }

        public string os
        {
            get
            {
                return "Windows";
            }
        }

        public string os_version
        {
            get
            {
                return "10";
            }
        }

        public string resolution
        {
            get
            {
                return "1920x1080";
            }
        }

        public string timezone
        {
            get
            {
                return "8";
            }
        }
    }
}
