using LeanCloud.Storage.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeanCloud.Realtime
{
    internal class OfflineMessageListener : IAVIMListener
    {
        private EventHandler<AVIMMesageEventArgs> m_OnOfflineMessageReceived;
        public event EventHandler<AVIMMesageEventArgs> OnOfflineMessageReceived
        {
            add
            {
                m_OnOfflineMessageReceived += value;
            }
            remove
            {
                m_OnOfflineMessageReceived -= value;
            }
        }
        public void OnNoticeReceived(AVIMNotice notice)
        {
            if (m_OnOfflineMessageReceived != null)
            {
                var msg =Json.Parse(notice.RawData["msg"].ToString()) as IDictionary<string, object>;
                var iMessage = AVRealtime.FreeStyleMessageClassingController.Instantiate(msg, notice.RawData);
                iMessage.Restore(notice.RawData);
                //var messageNotice = new AVIMMessageNotice(notice.RawData);
                //var messaegObj = AVIMMessage.Create(messageNotice);
                var args = new AVIMMesageEventArgs(iMessage);
                m_OnOfflineMessageReceived.Invoke(this, args);
            }

        }

        public bool ProtocolHook(AVIMNotice notice)
        {
            if (notice.CommandName != "direct") return false;
            if (!notice.RawData.ContainsKey("offline")) return false;
            return true;
        }
    }
}
