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
    /// 纯文本信息
    /// </summary>
    [AVIMMessageClassName("_AVIMTextMessage")]
    public class AVIMTextMessage : AVIMTypedMessage
    {
        /// <summary>
        /// 构建一个文本信息 <see cref="AVIMTextMessage"/> class.
        /// </summary>
        public AVIMTextMessage()
        {

        }

        /// <summary>
        /// 接受消息之后从服务端数据反序列成一个 AVIMTextMessage 对象
        /// </summary>
        /// <param name="messageNotice">来自服务端的消息通知</param>
        public AVIMTextMessage(AVIMMessageNotice messageNotice)
        {
            this.TextContent = messageNotice.RawMessage[AVIMProtocol.LCTEXT].ToString();
        }

        /// <summary>
        /// 文本内容
        /// </summary>
        [AVIMMessageFieldName("_lctext")]
        public string TextContent
        {
            get; set;
        }

        /// <summary>
        /// 文本内容
        /// </summary>
        [AVIMMessageFieldName("_lctype")]
        public int LCType
        {
            get; set;
        }

        /// <summary>
        /// 构造一个纯文本信息
        /// </summary>
        /// <param name="textContent"></param>
        public AVIMTextMessage(string textContent)
            : this()
        {
            LCType = -1;
            TextContent = textContent;
        }

        public override bool Validate(string msgStr)
        {
            if (!base.Validate(msgStr)) return false;
            var msg = Json.Parse(msgStr) as IDictionary<string, object>;
            
            return msg[AVIMProtocol.LCTYPE].ToString() == LCType.ToString();
        }
    }
}
