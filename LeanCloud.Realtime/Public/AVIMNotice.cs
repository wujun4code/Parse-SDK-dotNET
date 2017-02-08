using LeanCloud;
using LeanCloud.Storage.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LeanCloud.Realtime
{
    public interface IAVIMNotice
    {
        AVIMNotice Restore(IDictionary<string, object> estimatedData);
    }
    /// <summary>
    /// 从服务端接受到的通知
    /// <para>通知泛指消息，对话信息变更（例如加人和被踢等），服务器的 ACK，消息回执等</para>
    /// </summary>
    public class AVIMNotice : EventArgs
    {
        public AVIMNotice()
        {

        }
        public readonly string CommandName;
        public readonly IDictionary<string, object> RawData;
        public AVIMNotice(IDictionary<string, object> estimatedData)
        {
            this.RawData = estimatedData;
            this.CommandName = estimatedData["cmd"].ToString();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AVIMMessageNotice : AVIMNotice, IAVIMNotice
    {
        public AVIMMessageNotice()
        {

        }
        public AVIMMessageNotice(IDictionary<string, object> estimatedData)
            :base(estimatedData)
        {
            this.ConversationId = estimatedData["cid"].ToString();
            this.FromClientId = estimatedData["fromPeerId"].ToString();
            this.MessageId = estimatedData["id"].ToString();
            this.ApplicationId = estimatedData["appId"].ToString();
            this.RawMessage = Json.Parse(estimatedData["msg"].ToString()) as IDictionary<string, object>;
        }

        public readonly string ConversationId;
        public readonly string FromClientId;
        public readonly IDictionary<string, object> RawMessage;
        public readonly string MessageId;
        public readonly long Timestamp;
        public readonly bool Transient;
        public readonly string ApplicationId;
        public readonly bool IsOffline;
        public readonly bool HasMore;

        public AVIMNotice Restore(IDictionary<string, object> estimatedData)
        {
            return new AVIMMessageNotice(estimatedData);
        }
    }
}
