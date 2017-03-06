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
        public async void DoTest()
        {
            await Task.Delay(0);
        }

        [Test]
        [AsyncStateMachine(typeof(AnalyticsTests))]
        public Task TestSend()
        {
            return AVAnalytics.InitAsync(new PC()).ContinueWith(t =>
            {
                var anlytics = AVAnalytics.Current;
                anlytics.TrackAppOpened();
                var pageId = anlytics.BeginPage("nnnPage");
                anlytics.TrackEvent("nnnnClicked");
                var inputEventId = anlytics.BeginEvent("nnnnnPut", "username", new Dictionary<string, object>()
                {
                    { "dooooo",666}
                });
                anlytics.EndEvent(inputEventId, null);
                anlytics.EndPage(pageId);
                return anlytics.SendAsync();
            }).Unwrap().ContinueWith(s => 
            {
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

        public string display_name
        {
            get
            {
                return "Õ≥º∆≤‚ ‘";
            }
        }

        public string iid
        {
            get
            {
                return null;
            }
        }

        public bool is_jailbroken
        {
            get
            {
                return false;
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

        public string package_name
        {
            get
            {
                return "com.leancloud.pc";
            }
        }

        public string resolution
        {
            get
            {
                return "1920x1080";
            }
        }

        public string sv
        {
            get
            {
                return "1.1";
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
