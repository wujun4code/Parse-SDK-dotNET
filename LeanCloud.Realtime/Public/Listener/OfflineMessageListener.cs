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
                var messageNotice = new AVIMMessageNotice(notice.RawData);
                var messaegObj = AVIMMessage.Create(messageNotice);
                var args = new AVIMMesageEventArgs(messaegObj);
                m_OnOfflineMessageReceived(this,args);
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
