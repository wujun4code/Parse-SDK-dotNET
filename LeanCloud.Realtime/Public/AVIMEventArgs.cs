using System;
using System.Collections;
using System.Collections.Generic;

namespace LeanCloud.Realtime
{
    /// <summary>
    /// 
    /// </summary>
    public class AVIMEventArgs : EventArgs
    {
        public AVIMEventArgs()
        {

        }

        /// <summary>
        /// LeanCloud 服务端发往客户端消息通知
        /// </summary>
        public string Message { get; set; }
    }


    public class AVIMMesageEventArgs : EventArgs
    {
        public AVIMMesageEventArgs(AVIMMessageNotice raw)
        {
            MessageNotice = raw;
        }
        public AVIMMessageNotice MessageNotice { get; internal set; }
    }

    public class AVIMTextMessageEventArgs : EventArgs
    {
        public AVIMTextMessageEventArgs(AVIMTextMessage raw)
        {
            TextMessage = raw;
        }
        public AVIMTextMessage TextMessage { get; internal set; }
    }

    /// <summary>
    /// 当对话中有人加入时，触发 <seealso cref="AVIMMembersJoinListener.OnMembersJoined"/> 时所携带的事件参数
    /// </summary>
    public class AVIMOnMembersJoinedEventArgs : EventArgs
    {
        /// <summary>
        /// 加入到对话的 Client Id(s)
        /// </summary>
        public IEnumerable<string> JoinedMembers { get; internal set; }

        /// <summary>
        /// 邀请的操作人
        /// </summary>
        public string IvitedBy { get; internal set; }

        /// <summary>
        /// 此次操作针对的对话 Id
        /// </summary>
        public string ConversationId { get; internal set; }
    }

    /// <summary>
    /// 当对话中有人加入时，触发 AVIMMembersJoinListener<seealso cref="AVIMMembersLeftListener.OnMembersLeft"/> 时所携带的事件参数
    /// </summary>
    public class AVIMOnMembersLeftEventArgs : EventArgs
    {
        /// <summary>
        /// 离开对话的 Client Id(s)
        /// </summary>
        public IEnumerable<string> LeftMembers { get; internal set; }

        /// <summary>
        /// 踢出的操作人
        /// </summary>
        public string KickedBy { get; internal set; }

        /// <summary>
        /// 此次操作针对的对话 Id
        /// </summary>
        public string ConversationId { get; internal set; }
    }
}
