using LeanCloud.Realtime.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanCloud.Realtime
{
    /// <summary>
    /// 默认的消息监听器
    /// </summary>
    public class AVIMMessageListener : IAVIMListener
    {
        /// <summary>
        /// 默认的 AVIMMessageListener 只会监听 direct 协议，但是并不会触发针对消息类型的判断的监听器
        /// </summary>
        public AVIMMessageListener()
        {
            m_NoticeAction = (notice) =>
            {
                OnMessage(notice);
            };
            m_HookFilter = (notice) => notice.CommandName == "direct";
        }
        public bool ProtocolHook(AVIMNotice notice)
        {
            return this.ProtocolHookFunc.Invoke(notice);
        }
        public AVIMMessageListener(Action<AVIMNotice> noticeAction)
        {
            m_NoticeAction = noticeAction;
        }
        protected Func<AVIMNotice, bool> m_HookFilter;

        /// <summary>
        /// 使用 <see cref="Func{AVIMNotice, bool}"/>的方式来构建 <see cref="MessageListener.ProtocolHook(AVIMNotice)"/>
        /// </summary>
        public Func<AVIMNotice, bool> ProtocolHookFunc
        {
            get
            {
                return m_HookFilter;
            }

            set
            {
                m_HookFilter = value;
            }
        }

        protected Action<AVIMNotice> m_NoticeAction;
        /// <summary>
        /// 接收消息之后的事件回调
        /// </summary>
        public Action<AVIMNotice> NoticeAction
        {
            get
            {
                return m_NoticeAction;
            }

            set
            {
                m_NoticeAction = value;
            }
        }
        private EventHandler<AVIMMesageEventArgs> m_OnMessageReceieved;
        /// <summary>
        /// 接收到聊天消息的事件通知
        /// </summary>
        public event EventHandler<AVIMMesageEventArgs> OnMessageReceieved
        {
            add
            {
                m_OnMessageReceieved += value;
            }
            remove
            {
                m_OnMessageReceieved -= value;
            }
        }
        internal void OnMessage(AVIMNotice notice)
        {
            var messageNotice = new AVIMMessageNotice(notice.RawData);
            var args = new AVIMMesageEventArgs(messageNotice);
            if (m_OnMessageReceieved != null)
            {
                m_OnMessageReceieved.Invoke(this, args);
            }
        }

        public void OnNoticeReceived(AVIMNotice notice)
        {
            this.OnMessage(notice);
        }
    }

    /// <summary>
    /// 文本消息监听器
    /// </summary>
    public class AVIMTextMessageListener : AVIMMessageListener
    {
        /// <summary>
        /// 构建默认的文本消息监听器
        /// </summary>
        public AVIMTextMessageListener()
        {
            this.NoticeAction = (notice) =>
            {
                if (m_OnTextMessageReceieved != null)
                {
                    var messageNotice = new AVIMMessageNotice(notice.RawData);
                    var textMessage = new AVIMTextMessage(messageNotice);

                    m_OnTextMessageReceieved(this, new AVIMTextMessageEventArgs(textMessage));
                }
            };

            Func<AVIMNotice, bool> textMessageHookFilter = (notice) =>
            {
                if (notice.CommandName != "direct") return false;
                var messageNotice = new AVIMMessageNotice(notice.RawData);
                if (!messageNotice.RawMessage.Keys.Contains(AVIMProtocol.LCTYPE)) return false;
                var typInt = 0;
                int.TryParse(messageNotice.RawMessage[AVIMProtocol.LCTYPE].ToString(), out typInt);
                if (typInt != -1) return false;
                return true;
            };
            this.ProtocolHookFunc = textMessageHookFilter;
        }

        public AVIMTextMessageListener(Action<AVIMTextMessage> textMessageReceived)
        {
            OnTextMessageReceieved += (sender, textMessage) =>
            {
                textMessageReceived(textMessage.TextMessage);
            };
        }

        private EventHandler<AVIMTextMessageEventArgs> m_OnTextMessageReceieved;
        public event EventHandler<AVIMTextMessageEventArgs> OnTextMessageReceieved
        {
            add
            {
                m_OnTextMessageReceieved += value;
            }
            remove
            {
                m_OnTextMessageReceieved -= value;
            }
        }
    }
}
