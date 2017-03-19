using LeanCloud;
using LeanCloud.Core.Internal;
using LeanCloud.Storage.Internal;
using LeanCloud.Realtime.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud.Realtime
{
    /// <summary>
    /// 代表一个实时通信的终端用户
    /// </summary>
    public class AVIMClient
    {
        private readonly string clientId;
        private readonly AVRealtime _realtime;
        internal readonly object mutex = new object();
        internal AVRealtime LinkedRealtime
        {
            get { return _realtime; }
        }

        /// <summary>
        /// 单点登录所使用的 Tag
        /// </summary>
        public string Tag
        {
            get;
            private set;
        }

        /// <summary>
        /// 客户端的标识,在一个 Application 内唯一。
        /// </summary>
        public string ClientId
        {
            get { return clientId; }
        }

        //private EventHandler<AVIMNotice> m_OnNoticeReceived;
        ///// <summary>
        ///// 接收到服务器的命令时触发的事件
        ///// </summary>
        //public event EventHandler<AVIMNotice> OnNoticeReceived
        //{
        //    add
        //    {
        //        m_OnNoticeReceived += value;
        //    }
        //    remove
        //    {
        //        m_OnNoticeReceived -= value;
        //    }
        //}

        private EventHandler<AVIMMesageEventArgs> m_OnMessageReceived;
        /// <summary>
        /// 接收到聊天消息的事件通知
        /// </summary>
        public event EventHandler<AVIMMesageEventArgs> OnMessageReceived
        {
            add
            {
                m_OnMessageReceived += value;
            }
            remove
            {
                m_OnMessageReceived -= value;
            }
        }

        private EventHandler<AVIMSessionClosedEventArgs> m_OnSessionClosed;

        public event EventHandler<AVIMSessionClosedEventArgs> OnSessionClosed
        {
            add
            {
                m_OnSessionClosed += value;
            }
            remove
            {
                m_OnSessionClosed -= value;
            }
        }

        /// <summary>
        /// 创建 AVIMClient 对象
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="realtime"></param>
        internal AVIMClient(string clientId, AVRealtime realtime)
            : this(clientId, null, realtime)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="tag"></param>
        /// <param name="realtime"></param>
        internal AVIMClient(string clientId, string tag, AVRealtime realtime)
        {
            this.clientId = clientId;
            Tag = tag ?? tag;
            _realtime = realtime;
            if (this.LinkedRealtime.State == AVRealtime.Status.Online)
            {
                #region sdk 强制在接收到消息之后一定要向服务端回发 ack
                var ackListener = new AVIMMessageListener();
                ackListener.OnMessageReceived += AckListener_OnMessageReceieved;
                this.RegisterListener(ackListener);
                #endregion

                #region 默认要为当前 client 绑定一个消息的监听器，用作消息的事件通知
                var messageListener = new AVIMMessageListener();
                messageListener.OnMessageReceived += MessageListener_OnMessageReceived;
                this.RegisterListener(messageListener);
                #endregion

                #region 默认要为当前 client 绑定一个 session close 的监听器，用来监测单点登录冲突的事件通知
                var sessionListener = new SessionListener();
                sessionListener.OnSessionClosed += SessionListener_OnSessionClosed;
                #endregion
            }
        }

        private void SessionListener_OnSessionClosed(int arg1, string arg2, string arg3)
        {
            if (m_OnSessionClosed != null)
            {
                var args = new AVIMSessionClosedEventArgs()
                {
                    Code = arg1,
                    Reason = arg2,
                    Detail = arg3
                };
                m_OnSessionClosed(this, args);
            }
        }

        private void MessageListener_OnMessageReceived(object sender, AVIMMesageEventArgs e)
        {
            if (this.m_OnMessageReceived != null)
            {
                this.m_OnMessageReceived.Invoke(this, e);
            }
        }

        private void AckListener_OnMessageReceieved(object sender, AVIMMesageEventArgs e)
        {
            lock (mutex)
            {
                var ackCommand = new AckCommand().MessageId(e.Message.Id)
                    .ConversationId(e.Message.ConversationId)
                    .PeerId(this.ClientId);

                AVRealtime.AVIMCommandRunner.RunCommandAsync(ackCommand);
            }
        }
        #region listener 

        /// <summary>
        /// 注册 IAVIMListener
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="runtimeHook"></param>
        public void RegisterListener(IAVIMListener listener, Func<AVIMNotice, bool> runtimeHook = null)
        {
            _realtime.SubscribeNoticeReceived(listener, runtimeHook);
        }
        #endregion
        /// <summary>
        /// 创建对话
        /// </summary>
        /// <param name="conversation">对话</param>
        /// <param name="isUnique">是否创建唯一对话，当 isUnique 为 true 时，如果当前已经有相同成员的对话存在则返回该对话，否则会创建新的对话。该值默认为 false。</param>
        /// <returns></returns>
        public Task<AVIMConversation> CreateConversationAsync(AVIMConversation conversation, bool isUnique = true)
        {
            var cmd = new ConversationCommand()
                .Generate(conversation)
                .Unique(isUnique);

            var convCmd = cmd.Option("start")
                .PeerId(clientId);

            return LinkedRealtime.AttachSignature(convCmd, LinkedRealtime.SignatureFactory.CreateStartConversationSignature(this.clientId, conversation.MemberIds)).OnSuccess(_ =>
             {
                 return AVRealtime.AVIMCommandRunner.RunCommandAsync(convCmd).OnSuccess(t =>
                 {
                     var result = t.Result;
                     if (result.Item1 < 1)
                     {
                         conversation.MemberIds.Add(ClientId);
                         conversation = new AVIMConversation(source: conversation, creator: ClientId, isUnique: isUnique);
                         conversation.MergeFromPushServer(result.Item2);
                         conversation.CurrentClient = this;
                     }

                     return conversation;
                 });
             }).Unwrap();
        }

        /// <summary>
        /// 创建与目标成员的对话
        /// </summary>
        /// <param name="members">目标成员</param>
        /// <param name="isUnique">是否是唯一对话</param>
        /// <returns></returns>
        public Task<AVIMConversation> CreateConversationAsync(IList<string> members = null, bool isUnique = true, IDictionary<string, object> options = null)
        {
            var conversation = new AVIMConversation(members: members);
            if (options != null)
            {
                foreach (var key in options.Keys)
                {
                    conversation[key] = options[key];
                }
            }
            return CreateConversationAsync(conversation, isUnique);
        }

        /// <summary>
        /// 创建与目标成员的对话
        /// </summary>
        /// <param name="member">目标成员</param>
        /// <param name="isUnique">是否是唯一对话</param>
        /// <returns></returns>
        public Task<AVIMConversation> CreateConversationAsync(string member = "", bool isUnique = true, IDictionary<string, object> options = null)
        {
            var members = new List<string>() { member };

            return CreateConversationAsync(members, isUnique, options);
        }

        /// <summary>
        /// 创建聊天室（即：暂态对话）
        /// </summary>
        /// <param name="conversationName">聊天室名称</param>
        /// <returns></returns>
        public Task<AVIMConversation> CreateChatRoomAsync(string conversationName)
        {
            var conversation = new AVIMConversation() { Name = conversationName, IsTransient = true };
            return CreateConversationAsync(conversation);
        }

        /// <summary>
        /// 获取一个对话
        /// </summary>
        /// <param name="id">对话的 ID</param>
        /// <param name="noCache">从服务器获取</param>
        /// <returns></returns>
        public Task<AVIMConversation> GetConversationAsync(string id, bool noCache)
        {
            if (!noCache) return Task.FromResult(new AVIMConversation() { ConversationId = id, CurrentClient = this });
            else
            {
                return this.GetQuery().WhereEqualTo("objectId", id).FirstAsync();
            }
        }

        #region send message
        /// <summary>
        /// 向目标对话发送消息
        /// </summary>
        /// <param name="conversation">目标对话</param>
        /// <param name="message">消息体</param>
        /// <param name="receipt">是否需要送达回执</param>
        /// <param name="transient">是否是暂态消息，暂态消息不返回送达回执(ack)，不保留离线消息，不触发离线推送</param>
        /// <param name="priority">消息等级，默认是1，可选值还有 2 ，3</param>
        /// <param name="will">标记该消息是否为下线通知消息</param>
        /// <param name="pushData">如果消息的接收者已经下线了，这个字段的内容就会被离线推送到接收者
        /// <remarks>例如，一张图片消息的离线消息内容可以类似于：[您收到一条图片消息，点击查看] 这样的推送内容，参照微信的做法</remarks>
        /// <returns></returns>
        public Task<AVIMMessage> SendMessageAsync(
            AVIMConversation conversation,
            IAVIMMessage message,
            bool receipt = true,
            bool transient = false,
            int priority = 1,
            bool will = false,
            IDictionary<string, object> pushData = null)
        {
            return this.SendMessageAsync(conversation, message, new AVIMSendOptions()
            {
                Receipt = receipt,
                Transient = transient,
                Priority = priority,
                Will = will,
                PushData = pushData
            });
        }
        /// <summary>
        /// 向目标对话发送消息
        /// </summary>
        /// <param name="conversation">目标对话</param>
        /// <param name="message">消息体</param>
        /// <param name="options">消息的发送选项，包含了一些特殊的标记<see cref="AVIMSendOptions"/></param>
        /// <returns></returns>
        public Task<AVIMMessage> SendMessageAsync(
          AVIMConversation conversation,
          IAVIMMessage message,
          AVIMSendOptions options)
        {
            return message.MakeAsync().ContinueWith(s =>
            {
                var avMessage = s.Result;

                avMessage.ConversationId = conversation.ConversationId;
                avMessage.FromClientId = this.ClientId;
                avMessage.Receipt = options.Receipt;
                avMessage.Transient = options.Transient;
                avMessage.MessageIOType = AVIMMessageIOType.AVIMMessageIOTypeOut;
                avMessage.MessageStatus = AVIMMessageStatus.AVIMMessageStatusSending;

                if (options.PushData != null)
                {
                    avMessage.Attribute("pushData", Json.Encode(options.PushData));
                }

                var cmd = new MessageCommand()
               .Message(avMessage.EncodeJsonString())
               .ConvId(conversation.ConversationId)
               .Receipt(options.Receipt)
               .Transient(options.Transient)
               .Priority(options.Priority)
               .Will(options.Will)
               .PeerId(this.clientId);

                return AVRealtime.AVIMCommandRunner.RunCommandAsync(cmd).ContinueWith<AVIMMessage>(t =>
                {
                    if (t.IsFaulted)
                    {
                        throw t.Exception;
                    }
                    else
                    {
                        var response = t.Result.Item2;
                        avMessage.Id = response["uid"].ToString();
                        avMessage.ServerTimestamp = long.Parse(response["t"].ToString());
                        avMessage.MessageStatus = AVIMMessageStatus.AVIMMessageStatusSent;

                        return avMessage;
                    }
                });
            }).Unwrap();
        }
        #endregion

        #region mute & unmute
        /// <summary>
        /// 当前用户对目标对话进行静音操作
        /// </summary>
        /// <param name="conversation"></param>
        /// <returns></returns>
        public Task MuteConversationAsync(AVIMConversation conversation)
        {
            var convCmd = new ConversationCommand()
                .ConversationId(conversation.ConversationId)
                .Option("mute")
                .PeerId(this.ClientId);

            return AVRealtime.AVIMCommandRunner.RunCommandAsync(convCmd);
        }
        /// <summary>
        /// 当前用户对目标对话取消静音，恢复该对话的离线消息推送
        /// </summary>
        /// <param name="conversation"></param>
        /// <returns></returns>
        public Task UnmuteConversationAsync(AVIMConversation conversation)
        {
            var convCmd = new ConversationCommand()
                .ConversationId(conversation.ConversationId)
                .Option("unmute")
                .PeerId(this.ClientId);

            return AVRealtime.AVIMCommandRunner.RunCommandAsync(convCmd);
        }
        #endregion

        #region Conversation members operations
        internal Task OperateMembersAsync(AVIMConversation conversation, string action, string member = null, IEnumerable<string> members = null)
        {
            List<string> membersAsList = null;
            if (string.IsNullOrEmpty(conversation.ConversationId))
            {
                throw new Exception("conversation id 不可以为空。");
            }
            if (members == null)
            {
                membersAsList = new List<string>();
            }
            membersAsList = members.ToList();
            if (members.Count() == 0 && string.IsNullOrEmpty(member))
            {
                throw new Exception("加人或者踢人的时候，被操作的 member(s) 不可以为空。");
            }
            membersAsList.Add(member);
            var cmd = new ConversationCommand().ConversationId(conversation.ConversationId)
                .Members(members)
                .Option(action)
                .PeerId(clientId);
            return this.LinkedRealtime.AttachSignature(cmd, LinkedRealtime.SignatureFactory.CreateConversationSignature(conversation.ConversationId, ClientId, members, ConversationSignatureAction.Add));
        }
        #region Join
        /// <summary>
        /// 当前用户加入到目标的对话中
        /// </summary>
        /// <param name="conversation">目标对话</param>
        /// <returns></returns>
        public Task JoinAsync(AVIMConversation conversation)
        {
            return this.OperateMembersAsync(conversation, "add", this.ClientId);
        }
        #endregion

        #region Invite
        /// <summary>
        /// 直接将其他人加入到目标对话
        /// <remarks>被操作的人会在客户端会触发 OnInvited 事件,而已经存在于对话的用户会触发 OnMembersJoined 事件</remarks>
        /// </summary>
        /// <param name="conversation">目标对话</param>
        /// <param name="member">单个的 Client Id</param>
        /// <param name="members">Client Id 集合</param>
        /// <returns></returns>
        public Task InviteAsync(AVIMConversation conversation, string member = null, IEnumerable<string> members = null)
        {
            return this.OperateMembersAsync(conversation, "add", member, members);
        }
        #endregion

        #region Left
        /// <summary>
        /// 当前 Client 离开目标对话
        /// <remarks>可以理解为是 QQ 群的退群操作</remarks>
        /// <remarks></remarks>
        /// </summary>
        /// <param name="conversation">目标对话</param>
        /// <returns></returns>
        public Task LeftAsync(AVIMConversation conversation)
        {
            return this.OperateMembersAsync(conversation, "remove", this.ClientId);
        }
        #endregion

        #region Kick
        /// <summary>
        /// 从目标对话中剔除成员
        /// </summary>
        /// <param name="conversation">目标对话</param>
        /// <param name="member">被剔除的单个成员</param>
        /// <param name="members">被剔除的成员列表</param>
        /// <returns></returns>
        public Task KickAsync(AVIMConversation conversation, string member = null, IEnumerable<string> members = null)
        {
            return this.OperateMembersAsync(conversation, "add", member, members);
        }
        #endregion
        #endregion

        #region Query && Message history

        /// <summary>
        /// 获取对话的查询
        /// </summary>
        /// <returns></returns>
        public AVIMConversationQuery GetQuery()
        {
            return new AVIMConversationQuery(this);
        }

        #region load message history
        /// <summary>
        /// 查询目标对话的历史消息
        /// <remarks>不支持聊天室（暂态对话）</remarks>
        /// </summary>
        /// <param name="conversation">目标对话</param>
        /// <param name="beforeMessageId">从 beforeMessageId 开始向前查询（和 beforeTimeStampPoint 共同使用，为防止某毫秒时刻有重复消息）</param>
        /// <param name="afterMessageId"> 截止到某个 afterMessageId (不包含)</param>
        /// <param name="beforeTimeStampPoint">从 beforeTimeStampPoint 开始向前查询</param>
        /// <param name="afterTimeStampPoint">拉取截止到 afterTimeStampPoint 时间戳（不包含）</param>
        /// <param name="limit">拉取消息条数，默认值 20 条，可设置为 1 - 1000 之间的任意整数</param>
        /// <returns></returns>
        public Task<IEnumerable<AVIMMessage>> QueryMessageAsync(AVIMConversation conversation,
            string beforeMessageId = null,
            string afterMessageId = null,
            DateTime? beforeTimeStampPoint = null,
            DateTime? afterTimeStampPoint = null,
            int limit = 20)
        {
            var logsCmd = new AVIMCommand()
                .Command("logs")
                .Argument("cid", conversation.ConversationId)
                .Argument("l", limit);

            if (beforeMessageId != null)
            {
                logsCmd = logsCmd.Argument("mid", beforeMessageId);
            }
            if (afterMessageId != null)
            {
                logsCmd = logsCmd.Argument("tmid", afterMessageId);
            }
            if (beforeTimeStampPoint != null)
            {
                logsCmd = logsCmd.Argument("t", beforeTimeStampPoint.Value.UnixTimeStampSeconds());
            }
            if (afterTimeStampPoint != null)
            {
                logsCmd = logsCmd.Argument("tt", afterTimeStampPoint.Value.UnixTimeStampSeconds());
            }
            return AVRealtime.AVIMCommandRunner.RunCommandAsync(logsCmd).OnSuccess(t =>
            {
                var rtn = new List<AVIMMessage>();
                var result = t.Result.Item2;
                var logs = result["logs"] as List<object>;
                if (logs != null)
                {
                    foreach (var log in logs)
                    {
                        var logMap = log as IDictionary<string, object>;
                        if (logMap != null)
                        {
                            var msgMap = Json.Parse(logMap["data"].ToString()) as IDictionary<string, object>;
                            var messageObj = AVIMMessage.Create(msgMap);
                            messageObj.Restore(logMap);
                            rtn.Add(messageObj);
                        }
                    }
                }

                return rtn.AsEnumerable();
            });
        }
        #endregion
        #endregion

        #region log out
        /// <summary>
        /// 退出登录或者切换账号
        /// </summary>
        /// <returns></returns>
        public Task CloseAsync()
        {
            var cmd = new SessionCommand().Option("close");
            return AVRealtime.AVIMCommandRunner.RunCommandAsync(cmd);

        }
        #endregion
    }
}
