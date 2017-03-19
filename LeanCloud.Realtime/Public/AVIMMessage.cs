using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeanCloud;
using System.Reflection;
using LeanCloud.Storage.Internal;
using System.Threading;
using System.Collections;
using LeanCloud.Realtime.Internal;

namespace LeanCloud.Realtime
{
    /// <summary>
    /// 实时消息的核心基类，它是所有消息的父类
    /// </summary>
    [AVIMMessageClassName(0)]
    public class AVIMMessage : IAVIMMessage, IEnumerable<KeyValuePair<string, object>>
    {
        /// <summary>
        /// 默认的构造函数
        /// </summary>
        public AVIMMessage()
            : this(new Dictionary<string, object>())
        {

        }
        internal readonly object mutex = new object();
        internal AVIMMessage(IDictionary<string, object> messageRawData)
        {
            messageData = messageRawData;
        }

        internal AVIMMessage(AVIMMessageNotice messageNotice)
            :this(messageNotice.RawMessage)
        {
            this.ConversationId = messageNotice.ConversationId;
            this.FromClientId = messageNotice.FromClientId;
            this.Id = messageNotice.MessageId;
            this.Transient = messageNotice.Transient;
            this.ServerTimestamp = messageNotice.Timestamp;
            this.MessageIOType = AVIMMessageIOType.AVIMMessageIOTypeIn;
            this.MessageStatus = AVIMMessageStatus.AVIMMessageStatusNone;

            this.serverData = messageNotice.RawData;
        }
        /// <summary>
        /// 对话的Id
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// 发送消息的 ClientId
        /// </summary>
        public string FromClientId { get; set; }

        /// <summary>
        /// 消息在全局的唯一标识Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 是否需要送达回执
        /// </summary>
        public bool Receipt { get; internal set; }

        /// <summary>
        /// 是否为暂态消息
        /// </summary>
        public bool Transient { get; internal set; }

        /// <summary>
        /// 实际发送的消息体
        /// </summary>
        public virtual IDictionary<string, object> messageData { get; set; }

        /// <summary>
        /// 消息的状态
        /// </summary>
        public AVIMMessageStatus MessageStatus { get; set; }

        /// <summary>
        /// 消息的来源类型
        /// </summary>
        public AVIMMessageIOType MessageIOType { get; set; }

        /// <summary>
        /// 服务器端的时间戳
        /// </summary>
        public long ServerTimestamp { get; set; }


        internal string cmdId { get; set; }

        internal long rcpTimestamp { get; set; }

        internal IDictionary<string, object> serverData = new Dictionary<string, object>();

        internal IDictionary<string, object> serverState;

        /// <summary>
        /// 对当前消息对象做 JSON 编码
        /// </summary>
        /// <returns></returns>
        public virtual string EncodeJsonString()
        {
            return Json.Encode(messageData);
        }

        /// <summary>
        /// 添加属性，属性最后会被编码在 msg 字段内
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public virtual void Attribute(string key, object value)
        {
            this[key] = value;
        }

        public virtual Task<AVIMMessage> MakeAsync()
        {
            return Task.FromResult<AVIMMessage>(this);
        }

        public virtual void Restore(IDictionary<string, object> logData)
        {
            this.serverState = logData;
            if (logData.ContainsKey("timestamp"))
            {
                long timestamp = 0;
                if (long.TryParse(logData["timestamp"].ToString(), out timestamp))
                {
                    this.ServerTimestamp = timestamp;
                }
            }
            if (logData.ContainsKey("from"))
            {
                this.FromClientId = logData["from"].ToString();
            }
            if (logData.ContainsKey("msgId"))
            {
                this.Id = logData["msgId"].ToString();
            }
            if (logData.ContainsKey("data"))
            {
                var msgEncodeStr = logData["data"].ToString();
                this.messageData = Json.Parse(msgEncodeStr) as IDictionary<string, object>;
            }
        }

        public virtual void Restore(AVIMMessageNotice messageNotice)
        {
            this.ConversationId = messageNotice.ConversationId;
            this.FromClientId = messageNotice.FromClientId;
            this.Id = messageNotice.MessageId;
            this.Transient = messageNotice.Transient;
            this.ServerTimestamp = messageNotice.Timestamp;
            this.MessageIOType = AVIMMessageIOType.AVIMMessageIOTypeIn;
            this.MessageStatus = AVIMMessageStatus.AVIMMessageStatusNone;

            this.messageData = messageNotice.RawMessage;

            this.serverData = messageNotice.RawData;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            lock (mutex)
            {
                return this.messageData.GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock (mutex)
            {
                return this.messageData.GetEnumerator();
            }
        }

        public virtual object this[string key]
        {
            get
            {
                lock (mutex)
                {
                    return messageData[key];
                }

            }
            set
            {
                lock (mutex)
                {
                    if (messageData == null)
                    {
                        messageData = new Dictionary<string, object>();
                    }
                    messageData[key] = value;
                }
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                lock (mutex)
                {
                    return messageData.Keys;
                }
            }
        }

        #region register convertor for typed message
        internal static IMessageSubclassingController MessageSubclassingController
        {
            get
            {
                return AVIMCorePlugins.Instance.MessageSubclassingController;
            }
        }
        public static void RegisterSubclass<T>() where T : AVIMMessage, new()
        {
            MessageSubclassingController.RegisterSubclass(typeof(T));
        }

        /// <summary>
        /// 根据类型枚举创建消息类型的子类
        /// </summary>
        /// <param name="typeEnumeIntValue"></param>
        /// <returns></returns>
        public static AVIMMessage Create(int typeEnumeIntValue)
        {
            return MessageSubclassingController.Instantiate(typeEnumeIntValue);
        }

        internal static AVIMMessage Create(IDictionary<string, object> msgData)
        {
            int typeEnumIntValue = 0;
            if (msgData != null)
            {
                if (msgData.ContainsKey(AVIMProtocol.LCTYPE))
                {
                    int.TryParse(msgData[AVIMProtocol.LCTYPE].ToString(), out typeEnumIntValue);
                }
            }
            var messageObj = AVIMMessage.Create(typeEnumIntValue);
            return messageObj;
        }

        internal static AVIMMessage Create(AVIMMessageNotice messageNotice)
        {
            if (messageNotice.RawMessage == null) return null;
            var messageObj = AVIMMessage.Create(messageNotice.RawMessage);
            messageObj.Restore(messageNotice);
            return messageObj;
        }

        public static AVIMMessage CreateWithoutData(int typeEnumeIntValue, string messageId)
        {
            var result = Create(typeEnumeIntValue);
            result.Id = messageId;
            return result;
        }
        public static T Create<T>() where T : AVIMMessage
        {
            return (T)MessageSubclassingController.Instantiate(MessageSubclassingController.GetTypeEnumIntValue(typeof(T)));
        }
        #endregion
    }

    /// <summary>
    /// 消息的发送选项
    /// </summary>
    public struct AVIMSendOptions
    {
        /// <summary>
        /// 是否需要送达回执
        /// </summary>
        public bool Receipt;
        /// <summary>
        /// 是否是暂态消息，暂态消息不返回送达回执(ack)，不保留离线消息，不触发离线推送
        /// </summary>
        public bool Transient;
        /// <summary>
        /// 消息的优先级，默认是1，可选值还有 2|3
        /// </summary>
        public int Priority;
        /// <summary>
        /// 是否为 Will 类型的消息，这条消息会被缓存在服务端，一旦当前客户端下线，这条消息会被发送到对话内的其他成员
        /// </summary>
        public bool Will;

        /// <summary>
        /// 如果消息的接收者已经下线了，这个字段的内容就会被离线推送到接收者
        ///<remarks>例如，一张图片消息的离线消息内容可以类似于：[您收到一条图片消息，点击查看] 这样的推送内容，参照微信的做法</remarks> 
        /// </summary>
        public IDictionary<string, object> PushData;
    }

    ///// <summary>
    ///// 富媒体消息类型，用户自定义部分的枚举值默认从1开始。
    ///// </summary>
    //public enum AVIMMessageMediaType : int
    //{
    //    /// <summary>
    //    /// 未指定
    //    /// </summary>
    //    None = 0,
    //    /// <summary>
    //    /// 纯文本信息
    //    /// </summary>
    //    Text = -1,
    //    /// <summary>
    //    /// 图片信息
    //    /// </summary>
    //    Image = -2,
    //    /// <summary>
    //    /// 音频消息
    //    /// </summary>
    //    Audio = -3,
    //    /// <summary>
    //    /// 视频消息
    //    /// </summary>
    //    Video = -4,
    //    /// <summary>
    //    /// 地理位置消息
    //    /// </summary>
    //    Location = -5,
    //    /// <summary>
    //    /// 文件消息
    //    /// </summary>
    //    File = -6,
    //}

    /// <summary>
    /// 消息状态
    /// </summary>
    public enum AVIMMessageStatus : int
    {
        /// <summary>
        /// 未知状态
        /// </summary>
        AVIMMessageStatusNone = 0,
        /// <summary>
        /// 正在发送
        /// </summary>
        AVIMMessageStatusSending = 1,
        /// <summary>
        /// 已发送
        /// </summary>
        AVIMMessageStatusSent = 2,
        /// <summary>
        /// 已送达到对方客户端
        /// </summary>
        AVIMMessageStatusDelivered = 3,

        /// <summary>
        /// 对方已读
        /// </summary>
        AVIMMessageStatusRead = 4,

        /// <summary>
        /// 失败
        /// </summary>
        AVIMMessageStatusFailed = 99,
    }
    /// <summary>
    /// 消息的来源类别
    /// </summary>
    public enum AVIMMessageIOType : int
    {
        /// <summary>
        /// 收到的消息
        /// </summary>
        AVIMMessageIOTypeIn = 1,
        /// <summary>
        /// 发送的消息
        /// </summary>
        AVIMMessageIOTypeOut = 2,
    }
}
