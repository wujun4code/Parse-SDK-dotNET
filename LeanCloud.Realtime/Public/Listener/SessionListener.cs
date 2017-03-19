using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeanCloud.Realtime
{
    internal class SessionListener : IAVIMListener
    {
        private event Action<int, string, string> _onSessionClosed;
        public event Action<int, string, string> OnSessionClosed
        {
            add
            {
                _onSessionClosed += value;
            }
            remove
            {
                _onSessionClosed -= value;
            }
        }
        public void OnNoticeReceived(AVIMNotice notice)
        {
            var code = 0;
            int.TryParse(notice.RawData["code"].ToString(), out code);
            var reason = notice.RawData["reason"].ToString();
            var detail = notice.RawData["detail"].ToString();

            if (_onSessionClosed != null)
            {
                _onSessionClosed(code, reason, detail);
            }
        }

        public bool ProtocolHook(AVIMNotice notice)
        {
            if (notice.CommandName != "session") return false;
            if (!notice.RawData.ContainsKey("op")) return false;
            if (notice.RawData["op"].ToString() != "closed") return false;

            return true;
        }
    }
}
