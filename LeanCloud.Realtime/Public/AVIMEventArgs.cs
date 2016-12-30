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
}
