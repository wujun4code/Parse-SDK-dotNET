using LeanCloud.Realtime.Internal;
using LeanCloud.Storage.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanCloud.Realtime
{
    /// <summary>
    /// 默认的消息监听器，它主要承担的指责是回执的发送与用户自定义的监听器不冲突
    /// </summary>
    public class AVIMMessageListener : IAVIMListener
    {
        /// <summary>
        /// 默认的 AVIMMessageListener 只会监听 direct 协议，但是并不会触发针对消息类型的判断的监听器
        /// </summary>
        public AVIMMessageListener()
        {

        }
        public virtual bool ProtocolHook(AVIMNotice notice)
        {
            if (notice.CommandName != "direct") return false;
            if (notice.RawData.ContainsKey("offline")) return false;
            return true;
        }

        private EventHandler<AVIMMesageEventArgs> m_OnMessageReceived;
        /// <summary>
        /// 接收到聊天消息的事件通知
        /// </summary>
        public event EventHandler<AVIMMesageEventArgs> OnMessageReceived
        {
            add
            {
                m_OnMessageReceived += value;
            }
            remove
            {
                m_OnMessageReceived -= value;
            }
        }
        internal virtual void OnMessage(AVIMNotice notice)
        {
            if (m_OnMessageReceived != null)
            {
                var msg = Json.Parse(notice.RawData["msg"].ToString()) as IDictionary<string, object>;
                var iMessage = AVRealtime.FreeStyleMessageClassingController.Instantiate(msg, notice.RawData);
                //var messageNotice = new AVIMMessageNotice(notice.RawData);
                //var messaegObj = AVIMMessage.Create(messageNotice);
                var args = new AVIMMesageEventArgs(iMessage);
                m_OnMessageReceived.Invoke(this, args);
            }
        }

        public virtual void OnNoticeReceived(AVIMNotice notice)
        {
            this.OnMessage(notice);
        }

    }

    /// <summary>
    /// 文本消息监听器
    /// </summary>
    public class AVIMTextMessageListener : IAVIMListener
    {
        /// <summary>
        /// 构建默认的文本消息监听器
        /// </summary>
        public AVIMTextMessageListener()
        {

        }

        /// <summary>
        /// 构建文本消息监听者
        /// </summary>
        /// <param name="textMessageReceived"></param>
        public AVIMTextMessageListener(Action<AVIMTextMessage> textMessageReceived)
        {
            OnTextMessageReceived += (sender, textMessage) =>
            {
                textMessageReceived(textMessage.TextMessage);
            };
        }

        private EventHandler<AVIMTextMessageEventArgs> m_OnTextMessageReceived;
        public event EventHandler<AVIMTextMessageEventArgs> OnTextMessageReceived
        {
            add
            {
                m_OnTextMessageReceived += value;
            }
            remove
            {
                m_OnTextMessageReceived -= value;
            }
        }

        public virtual bool ProtocolHook(AVIMNotice notice)
        {
            if (notice.CommandName != "direct") return false;
            var messageNotice = new AVIMMessageNotice(notice.RawData);
            if (!messageNotice.RawMessage.Keys.Contains(AVIMProtocol.LCTYPE)) return false;
            var typInt = 0;
            int.TryParse(messageNotice.RawMessage[AVIMProtocol.LCTYPE].ToString(), out typInt);
            if (typInt != -1) return false;
            return true;
        }

        public virtual void OnNoticeReceived(AVIMNotice notice)
        {
            if (m_OnTextMessageReceived != null)
            {
                var textMessage = this.Encode(notice);
                m_OnTextMessageReceived(this, new AVIMTextMessageEventArgs(textMessage));
            }
        }

        public AVIMTextMessage Encode(AVIMNotice notice)
        {
            var messageNotice = new AVIMMessageNotice(notice.RawData);
            var textMessage = new AVIMTextMessage(messageNotice);
            return textMessage;
        }
    }
}
