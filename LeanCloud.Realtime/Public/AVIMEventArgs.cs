using System;

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
}
