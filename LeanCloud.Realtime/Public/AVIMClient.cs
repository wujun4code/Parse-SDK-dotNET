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

        private EventHandler<AVIMNotice> m_OnNoticeReceived;
        /// <summary>
        /// 接收到服务器的命令时激发的事件
        /// </summary>
        public event EventHandler<AVIMNotice> OnNoticeReceived
        {
            add
            {
                m_OnNoticeReceived += value;
            }
            remove
            {
                m_OnNoticeReceived -= value;
            }
        }

        //private EventHandler<AVIMMesageEventArgs> m_OnMessageReceieved;
        ///// <summary>
        ///// 接收到聊天消息的事件通知
        ///// </summary>
        //public event EventHandler<AVIMMesageEventArgs> OnMessageReceieved
        //{
        //    add
        //    {
        //        m_OnMessageReceieved += value;
        //    }
        //    remove
        //    {
        //        m_OnMessageReceieved -= value;
        //    }
        //}


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
        }

        /// <summary>
        /// 注册 IAVIMListener
        /// </summary>
        /// <param name="listener"></param>
        public void RegisterListener(IAVIMListener listener)
        {
            _realtime.SubscribeNoticeReceived(listener);
        }
      
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
                .AppId(AVClient.CurrentConfiguration.ApplicationId)
                .PeerId(clientId);

            return LinkedRealtime.AttachSignature(convCmd, LinkedRealtime.SignatureFactory.CreateStartConversationSignature(this.clientId, conversation.MemberIds)).OnSuccess(_ =>
             {
                 return AVRealtime.AVCommandRunner.RunCommandAsync(convCmd).OnSuccess(t =>
                 {
                     var result = t.Result;
                     if (result.Item1 < 1)
                     {
                         conversation.MemberIds.Add(ClientId);
                         conversation = new AVIMConversation(source: conversation, creator: ClientId);
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
            var conversation = new AVIMConversation() { Name = conversationName, IsTransient = false };
            return CreateConversationAsync(conversation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="noCache"></param>
        /// <returns></returns>
        public Task<AVIMConversation> GetConversation(string id, bool noCache)
        {
            if (!noCache) return Task.FromResult(new AVIMConversation() { ConversationId = id });
            else
            {
                return Task.FromResult(new AVIMConversation() { ConversationId = id });
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<AVIMMessage> SendMessageAsync(AVIMConversation conversation, IAVIMMessage message)
        {
            return message.MakeAsync().ContinueWith(s =>
            {
                var avMessage = s.Result;
                var cmd = new MessageCommand()
               .Message(avMessage.EncodeJsonString())
               .ConvId(conversation.ConversationId)
               .Receipt(avMessage.Receipt)
               .Transient(avMessage.Transient)
               .AppId(AVClient.CurrentConfiguration.ApplicationId)
               .PeerId(this.clientId);

                return AVRealtime.AVCommandRunner.RunCommandAsync(cmd).ContinueWith<AVIMMessage>(t =>
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

                        return avMessage;
                    }
                });
            }).Unwrap();

        }

        /// <summary>
        /// 获取对话的查询
        /// </summary>
        /// <returns></returns>
        public AVIMConversationQuery GetQuery()
        {
            return new AVIMConversationQuery(this);
        }
    }
}
