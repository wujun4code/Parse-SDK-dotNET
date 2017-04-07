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
using LeanCloud.Core.Internal;

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
        /// <summary>
        /// 根据字典创建一个消息
        /// </summary>
        /// <param name="body"></param>
        public AVIMMessage(IDictionary<string, object> body)
        {
            Body = body;
        }

        internal AVIMMessage(AVIMMessageNotice messageNotice)
            : this(messageNotice.RawMessage)
        {
            this.ConversationId = messageNotice.ConversationId;
            this.FromClientId = messageNotice.FromClientId;
            this.Id = messageNotice.MessageId;
            this.ServerTimestamp = messageNotice.Timestamp;
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
        /// 实际发送的消息体
        /// </summary>
        public virtual IDictionary<string, object> Body { get; set; }

        ///// <summary>
        ///// 消息的状态
        ///// </summary>
        //public AVIMMessageStatus MessageStatus { get; set; }

        ///// <summary>
        ///// 消息的来源类型
        ///// </summary>
        //public AVIMMessageIOType MessageIOType { get; set; }

        /// <summary>
        /// 服务器端的时间戳
        /// </summary>
        public long ServerTimestamp { get; set; }

        /// <summary>
        /// 对方收到消息的时间戳，如果是多人聊天，那以最早收到消息的人回发的 ACK 为准
        /// </summary>
        public long RcpTimestamp { get; set; }

        internal string cmdId { get; set; }

        /// <summary>
        /// 对当前消息对象做 JSON 编码
        /// </summary>
        /// <returns></returns>
        public virtual string EncodeJsonString()
        {
            var avEncodedBody = this.ToJSONObjectForSending();
            return Json.Encode(avEncodedBody);
        }
        internal virtual IDictionary<string, object> DecodeJsonObject(IDictionary<string, object> msg)
        {
            var result = new Dictionary<string, object>();
            foreach (var pair in msg)
            {
                var operation = pair.Value;
                result[pair.Key] = AVDecoder.Instance.Decode(operation);
            }
            return result;
        }
        internal IDictionary<string, object> ToJSONObjectForSending()
        {
            var result = new Dictionary<string, object>();
            foreach (var pair in Body)
            {
                // Serialize the data
                var operation = pair.Value;

                result[pair.Key] = PointerOrLocalIdEncoder.Instance.Encode(operation);
            }
            return result;
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

        public virtual Task<IDictionary<string, object>> MakeAsync()
        {
            return Task.FromResult<IDictionary<string, object>>(this.Body);
        }

        //public virtual void Convert(IDictionary<string, object> logData)
        //{
        //    this.serverState = logData;
        //    if (logData.ContainsKey("timestamp"))
        //    {
        //        long timestamp = 0;
        //        if (long.TryParse(logData["timestamp"].ToString(), out timestamp))
        //        {
        //            this.ServerTimestamp = timestamp;
        //        }
        //    }
        //    if (logData.ContainsKey("from"))
        //    {
        //        this.FromClientId = logData["from"].ToString();
        //    }
        //    if (logData.ContainsKey("msgId"))
        //    {
        //        this.Id = logData["msgId"].ToString();
        //    }
        //    if (logData.ContainsKey("data"))
        //    {
        //        var msgEncodeStr = logData["data"].ToString();
        //        this.Body = Json.Parse(msgEncodeStr) as IDictionary<string, object>;
        //    }
        //}

        public virtual void Convert(AVIMMessageNotice messageNotice)
        {
            this.ConversationId = messageNotice.ConversationId;
            this.FromClientId = messageNotice.FromClientId;
            this.Id = messageNotice.MessageId;
            this.ServerTimestamp = messageNotice.Timestamp;

            var avDecode = AVDecoder.Instance.Decode(messageNotice.RawMessage) as IDictionary<string, object>;
            this.Body = avDecode;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            lock (mutex)
            {
                return this.Body.GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock (mutex)
            {
                return this.Body.GetEnumerator();
            }
        }

        public virtual object this[string key]
        {
            get
            {
                lock (mutex)
                {
                    return Body[key];
                }

            }
            set
            {
                lock (mutex)
                {
                    if (Body == null)
                    {
                        Body = new Dictionary<string, object>();
                    }
                    Body[key] = value;
                }
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                lock (mutex)
                {
                    return Body.Keys;
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
            AVRealtime.FreeStyleMessageClassingController.RegisterSubclass(typeof(T));
        }

        ///// <summary>
        ///// 根据类型枚举创建消息类型的子类
        ///// </summary>
        ///// <param name="typeEnumeIntValue"></param>
        ///// <returns></returns>
        //public static AVIMMessage Create(int typeEnumeIntValue)
        //{
        //    return MessageSubclassingController.Instantiate(typeEnumeIntValue);
        //}

        //internal static AVIMMessage Create(IDictionary<string, object> msgData)
        //{
        //    int typeEnumIntValue = 0;
        //    if (msgData != null)
        //    {
        //        if (msgData.ContainsKey(AVIMProtocol.LCTYPE))
        //        {
        //            int.TryParse(msgData[AVIMProtocol.LCTYPE].ToString(), out typeEnumIntValue);
        //        }
        //    }
        //    var messageObj = AVIMMessage.Create(typeEnumIntValue);
        //    return messageObj;
        //}

        //internal static AVIMMessage Create(AVIMMessageNotice messageNotice)
        //{
        //    if (messageNotice.RawMessage == null) return null;
        //    var messageObj = AVIMMessage.Create(messageNotice.RawMessage);
        //    messageObj.Convert(messageNotice);
        //    return messageObj;
        //}

        //public static AVIMMessage CreateWithoutData(int typeEnumeIntValue, string messageId)
        //{
        //    var result = Create(typeEnumeIntValue);
        //    result.Id = messageId;
        //    return result;
        //}
        public static T Create<T>() where T : AVIMMessage
        {
            return (T)MessageSubclassingController.Instantiate(MessageSubclassingController.GetTypeEnumIntValue(typeof(T)));
        }

        public virtual bool Validate(IDictionary<string, object> msg)
        {
            if (msg == null) return false;
            if (!msg.ContainsKey(AVIMProtocol.LCTYPE)) return false;
            int typeEnumIntValue = 0;
            if (msg.ContainsKey(AVIMProtocol.LCTYPE))
            {
                int.TryParse(msg[AVIMProtocol.LCTYPE].ToString(), out typeEnumIntValue);
            }
            if (typeEnumIntValue != -1) return false;
            return true;
        }

        public virtual IAVIMMessage Restore(IDictionary<string, object> msg)
        {
            this.Body = DecodeJsonObject(msg);
            return this;
        }
        #endregion
    }

    /// <summary>
    /// 消息的发送选项
    /// </summary>
    public struct AVIMSendOptions
    {
        private bool _receipt;
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
        AVIMMessageIOTypeOut = 2
    }
}
