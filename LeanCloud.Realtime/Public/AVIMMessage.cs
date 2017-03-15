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

        internal AVIMMessage(IDictionary<string, object> messageRawData)
        {
            messageData = messageRawData;
        }

        internal AVIMMessage(AVIMMessageNotice messageNotice)
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
        internal readonly Object mutexObj = new Object();
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
        /// 是否需要回执
        /// </summary>
        public bool Receipt { get; set; }

        /// <summary>
        /// 是否为暂态消息
        /// </summary>
        public bool Transient { get; set; }

        /// <summary>
        /// 
        /// </summary>
        //public string MimeType { get; set; }

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

        internal readonly IDictionary<string, object> serverData = new Dictionary<string, object>();

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
                this.FromClientId = logData["timestamp"].ToString();
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

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            lock (mutexObj)
            {
                return this.messageData.GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock (mutexObj)
            {
                return this.messageData.GetEnumerator();
            }
        }

        public virtual object this[string key]
        {
            get
            {
                return messageData[key];
            }
            set
            {
                if (messageData == null)
                {
                    messageData = new Dictionary<string, object>();
                }
                messageData[key] = value;
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

        public static AVIMMessage Create(int typeEnumeIntValue)
        {
            return MessageSubclassingController.Instantiate(typeEnumeIntValue);
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
        /// 
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
        /// 已送达
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
