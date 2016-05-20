using LeanMessage.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanMessage
{
    /// <summary>
    /// 纯文本信息
    /// </summary>
    public class AVIMTextMessage : AVIMTypedMessage
    {
        /// <summary>
        /// 构建一个文本信息 <see cref="AVIMTextMessage"/> class.
        /// </summary>
        public AVIMTextMessage()
        {
            this.MediaType = AVIMMessageMediaType.Text;
        }
        /// <summary>
        /// 文本内容
        /// </summary>
        public string TextContent { get; set; }

        /// <summary>
        /// 构造一个纯文本信息
        /// </summary>
        /// <param name="textContent"></param>
        public AVIMTextMessage(string textContent)
            : this()
        {
            TextContent = textContent;
        }

        public override string EncodeJsonString()
        {
            this.Attribute(AVIMProtocol.LCTYPE,-1);
            this.Attribute(AVIMProtocol.LCTEXT, TextContent);
            return base.EncodeJsonString();
        }
    }
}
