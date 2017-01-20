using LeanCloud.Realtime.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanCloud.Realtime
{

    public interface IAVIMNotice
    {
        AVIMNotice Restore(IDictionary<string, object> estimatedData);
    }

    public interface IAVIMListener
    {
        Func<AVIMNotice, bool> HookFilter { get; set; }
        Action<AVIMNotice> NoticeAction { get; set; }

        IAVIMListener Pipe(IAVIMListener after);
    }

    public class MessageListener : IAVIMListener
    {
        public MessageListener()
        {
            m_NoticeAction = (notice) =>
            {
                OnMessage(notice);
            };
        }

        public MessageListener(Action<AVIMNotice> noticeAction)
        {
            m_NoticeAction = noticeAction;
        }
        protected Func<AVIMNotice, bool> m_HookFilter;
        public Func<AVIMNotice, bool> HookFilter
        {
            get
            {
                return (notice) => notice.CommandName == "direct";
            }

            set
            {
                m_HookFilter = value;
            }
        }

        protected Action<AVIMNotice> m_NoticeAction;
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

        public IAVIMListener Pipe(IAVIMListener after)
        {
            Func<AVIMNotice, bool> a = (AVIMNotice notice) =>
             {
                 if (m_HookFilter(notice))
                 {
                     return after.HookFilter(notice);
                 }
                 return false;
             };
            this.m_HookFilter = a;
            return this;
        }

        public Func<AVIMNotice, bool> Merge(IAVIMListener after)
        {
            Func<AVIMNotice, bool> a = (AVIMNotice notice) =>
            {
                if (this.HookFilter(notice))
                {
                    return after.HookFilter(notice);
                }
                return false;
            };
            return a;
        }

    }

    public class TextMessageListener : MessageListener
    {
        public TextMessageListener()
            : base()
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
            this.m_HookFilter = base.Merge(this);
        }

        public TextMessageListener(Action<AVIMTextMessage> textMessageReceived)
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

        public Func<AVIMNotice, bool> HookFilter
        {
            get
            {
                return (notice) =>
                {
                    var messageNotice = new AVIMMessageNotice(notice.RawData);
                    if (!messageNotice.RawMessage.Keys.Contains(AVIMProtocol.LCTYPE)) return false;
                    var typInt = 0;
                    int.TryParse(messageNotice.RawMessage[AVIMProtocol.LCTYPE].ToString(), out typInt);
                    if (typInt != -1) return false;
                    return true;
                };
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public Action<AVIMNotice> NoticeAction
        {
            get;
            set;
        }
    }
}
