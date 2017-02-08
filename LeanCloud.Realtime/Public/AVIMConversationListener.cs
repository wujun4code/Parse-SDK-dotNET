﻿using LeanCloud.Realtime.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanCloud.Realtime
{
    /// <summary>
    /// Conversation 相关的事件监听器
    /// </summary>
    public class AVIMConversationListener : IAVIMListener
    {
        private static readonly string[] listeningCommandNames = new string[] {
           "joined", "left", "members-joined", "members-left", "added", "removed"
        };
        protected bool IsCommandNameListening(string key)
        {
            return listeningCommandNames.Contains(key);
        }
        private EventHandler<AVIMOnMembersChangedEventArgs> m_OnConversationMembersChanged;
        /// <summary>
        /// 每一个当前 Client 所属的对话产生人员变动，都会触发 OnConversationMembersChanged 事件。
        /// </summary>
        public event EventHandler<AVIMOnMembersChangedEventArgs> OnConversationMembersChanged
        {
            add
            {
                m_OnConversationMembersChanged += value;
            }
            remove
            {
                m_OnConversationMembersChanged -= value;
            }
        }
        public void OnNoticeReceived(AVIMNotice notice)
        {
            var cid = notice.RawData["cid"].ToString();
            var conversation = new AVIMConversation(cid);

            var args = new AVIMOnMembersChangedEventArgs()
            {

            };

            var generators = new Func<AVIMNotice, AVIMOnMembersChangedEventArgs>[]
            {
                (e)=>
                {
                    return new AVIMOnMembersChangedEventArgs()
                    {
                        Conversation = conversation,
                        AffectedType= AVIMConversationEventType.Joined,
                        Oprator = e.RawData["initBy"].ToString(),
                        OpratedTime = DateTime.Now
                    };
                },
                (e)=>
                {
                    return new AVIMOnMembersChangedEventArgs()
                    {
                        Conversation = conversation,
                        AffectedType= AVIMConversationEventType.Left,
                        Oprator =e.RawData["initBy"].ToString(),
                        OpratedTime = DateTime.Now
                    };
                },
                (e)=>
                {
                    return new AVIMOnMembersChangedEventArgs()
                    {
                        Conversation = conversation,
                        AffectedType= AVIMConversationEventType.MembersJoined,
                        Oprator = e.RawData["initBy"].ToString(),
                        AffectedMembers =new List<string>(e.RawData["m"] as string[])
                    };
                },
                (e)=>
                {
                    return new AVIMOnMembersChangedEventArgs()
                    {
                        Conversation = conversation,
                        AffectedType = AVIMConversationEventType.MembersLeft,
                        Oprator =e.RawData["initBy"].ToString(),
                        AffectedMembers =new List<string>(e.RawData["m"] as string[]),
                        OpratedTime = DateTime.Now
                    };

                },
                (e)=>
                {
                    var rtn= new AVIMOnMembersChangedEventArgs()
                    {
                        Conversation = conversation,
                        AffectedType = AVIMConversationEventType.Invited,
                        AffectedMembers = new List<string>(e.RawData["m"] as string[]),
                        OpratedTime=DateTime.Now
                    };
                    return rtn;
                },
                (e)=>
                {
                    var rtn = new AVIMOnMembersChangedEventArgs()
                    {
                        Conversation = conversation,
                        AffectedType = AVIMConversationEventType.Kicked,
                        AffectedMembers = new List<string>(e.RawData["m"] as string[]),
                        OpratedTime = DateTime.Now
                    };

                    return rtn;
                }
            };

            var op = notice.RawData["op"].ToString();
            int ssIndex = Array.IndexOf(listeningCommandNames, op);
            var action = generators[ssIndex];
            args = action(notice);
            if (m_OnConversationMembersChanged != null)
            {
                m_OnConversationMembersChanged(this, args);
            }
        }


        public bool ProtocolHook(AVIMNotice notice)
        {
            if (notice.CommandName != "conv") return false;
            if (!this.IsCommandNameListening(notice.CommandName)) return false;
            return true;
        }
    }

    /// <summary>
    /// 对话中成员变动的事件参数，它提供被操作的对话（Conversation），操作类型（AffectedType）
    /// 受影响的成员列表（AffectedMembers）
    /// </summary>
    public class AVIMOnMembersChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 本次成员变动中被操作的具体对话（AVIMConversation）的对象
        /// </summary>
        public AVIMConversation Conversation { get; set; }

        /// <summary>
        /// 变动的类型
        /// </summary>
        public AVIMConversationEventType AffectedType { get; internal set; }

        /// <summary>
        /// 受影响的成员的 Client Ids
        /// </summary>
        public IList<string> AffectedMembers { get; set; }

        /// <summary>
        /// 操作人的 Client Id
        /// </summary>
        public string Oprator { get; set; }

        /// <summary>
        /// 操作的时间，已转化为本地时间
        /// </summary>
        public DateTime OpratedTime { get; set; }
    }

    /// <summary>
    /// 变动的类型，目前支持如下：
    /// 1、Joined：当前 Client 主动加入，案例：当 A 主动加入到对话，A 将收到 Joined 事件响应，其余的成员收到 MembersJoined 事件响应
    /// 2、Left：当前 Client 主动退出，案例：当 A 从对话中退出，A 将收到 Left 事件响应，其余的成员收到 MembersLeft 事件响应
    /// 3、MembersJoined：某个成员加入（区别于Joined和Kicked），案例：当 A 把 B 加入到对话中，C 将收到 MembersJoined 事件响应
    /// 4、MembersLeft：某个成员加入（区别于Joined和Kicked），案例：当 A 把 B 从对话中剔除，C 将收到 MembersLeft 事件响应
    /// 5、Invited：当前 Client 被邀请加入，案例：当 A 被 B 邀请加入到对话中，A 将收到 Invited 事件响应，B 将收到 Joined ，其余的成员收到 MembersJoined 事件响应
    /// 6、Kicked：当前 Client 被剔除，案例：当 A 被 B 从对话中剔除，A 将收到 Kicked 事件响应，B 将收到 Left，其余的成员收到 MembersLeft 事件响应
    /// </summary>
    public enum AVIMConversationEventType
    {
        Joined = 1,
        Left,
        MembersJoined,
        MembersLeft,
        Invited,
        Kicked
    }

}
