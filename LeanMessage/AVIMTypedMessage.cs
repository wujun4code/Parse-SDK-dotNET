using LeanCloud;
using LeanMessage.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanMessage
{
    /// <summary>
    /// 富媒体消息，它是所有富媒体的父类，所有自定义的富媒体消息都应该继承自它。
    /// </summary>
    public abstract class AVIMTypedMessage : AVIMMessage
    {
        public AVIMTypedMessage()
        {

        }
        public AVIMTypedMessage(AVIMMessage avMessage)
        {

        }


        /// <summary>
        /// 富媒体消息类型
        /// </summary>
        public virtual AVIMMessageMediaType MediaType { get; set; }

        /// <summary>
        /// 自定义属性
        /// </summary>
        public IDictionary<string, object> Attributes { get; set; }

        internal IDictionary<string, object> typedMessageBody;

        /// <summary>
        /// 富媒体消息的标题
        /// </summary>
        public string Title { get; set; }


    }
}
