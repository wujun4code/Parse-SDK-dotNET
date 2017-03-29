﻿using LeanCloud.Realtime.Internal;
using LeanCloud.Storage.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud.Realtime
{
    /// <summary>
    /// 对话查询类
    /// </summary>
    public class AVIMConversationQuery : AVQueryBase<AVIMConversationQuery, AVIMConversation>
    {
        internal AVIMClient CurrentClient { get; set; }
        internal AVIMConversationQuery(AVIMClient _currentClient)
            : base()
        {
            CurrentClient = _currentClient;
        }

        bool compact;
        bool withLastMessageRefreshed;

        private AVIMConversationQuery(AVIMConversationQuery source,
            IDictionary<string, object> where = null,
            IEnumerable<string> replacementOrderBy = null,
            IEnumerable<string> thenBy = null,
            int? skip = null,
            int? limit = null,
            IEnumerable<string> includes = null,
            IEnumerable<string> selectedKeys = null,
            String redirectClassNameForKey = null)
            : base(source, where, replacementOrderBy, thenBy, skip, limit, includes, selectedKeys, redirectClassNameForKey)
        {

        }

        internal override AVIMConversationQuery CreateInstance(
            AVQueryBase<AVIMConversationQuery, AVIMConversation> source,
            IDictionary<string, object> where,
            IEnumerable<string> replacementOrderBy,
            IEnumerable<string> thenBy,
            int? skip,
            int? limit,
            IEnumerable<string> includes,
            IEnumerable<string> selectedKeys = null,
            String redirectClassNameForKey = null)
        {
            var rtn = new AVIMConversationQuery(this, where, replacementOrderBy, thenBy, skip, limit, includes);
            rtn.CurrentClient = this.CurrentClient;
            rtn.compact = this.compact;
            rtn.withLastMessageRefreshed = this.withLastMessageRefreshed;
            return rtn;
        }

        internal override AVIMConversationQuery CreateInstance(AVIMConversationQuery source,
            IDictionary<string, object> where = null,
            IEnumerable<string> replacementOrderBy = null,
            IEnumerable<string> thenBy = null,
            int? skip = null,
            int? limit = null,
            IEnumerable<string> includes = null,
            IEnumerable<string> selectedKeys = null,
            String redirectClassNameForKey = null)
        {
            var rtn = new AVIMConversationQuery(this, where, replacementOrderBy, thenBy, skip, limit, includes, selectedKeys, redirectClassNameForKey);
            rtn.CurrentClient = this.CurrentClient;
            rtn.compact = this.compact;
            rtn.withLastMessageRefreshed = this.withLastMessageRefreshed;
            return rtn;
        }
        internal AVIMCommand GenerateQueryCommand()
        {
            var cmd = new ConversationCommand();

            var queryParameters = this.BuildParameters(false);
            if (queryParameters != null)
            {
                if (queryParameters.Keys.Contains("where"))
                    cmd.Where(queryParameters["where"]);

                if (queryParameters.Keys.Contains("skip"))
                    cmd.Skip(int.Parse(queryParameters["skip"].ToString()));

                if (queryParameters.Keys.Contains("limit"))
                    cmd.Limit(int.Parse(queryParameters["limit"].ToString()));

                if (queryParameters.Keys.Contains("sort"))
                    cmd.Sort(queryParameters["order"].ToString());
            }

            return cmd.Option("query").PeerId(CurrentClient.ClientId);
        }

        public AVIMConversationQuery WithLastMessageRefreshed(bool enabled)
        {
            this.withLastMessageRefreshed = enabled;
            return CreateInstance(this);
        }
        public AVIMConversationQuery Compact(bool enabled)
        {
            this.compact = enabled;
            return CreateInstance(this);
        }
        
        public override Task<int> CountAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 查找符合条件的对话
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<IEnumerable<AVIMConversation>> FindAsync(CancellationToken cancellationToken)
        {
            var convCmd = this.GenerateQueryCommand();
            return AVRealtime.AVIMCommandRunner.RunCommandAsync(convCmd).OnSuccess(t => 
            {
                var result = t.Result.Item2;

                IList<AVIMConversation> rtn = new List<AVIMConversation>();
                var conList = result["results"] as IList<object>;
                if (conList != null)
                {
                    foreach (var c in conList)
                    {
                        var cData = c as IDictionary<string, object>;
                        if (cData != null)
                        {
                            var con = AVIMConversation.CreateWithData(cData, CurrentClient);
                            rtn.Add(con);
                        }
                    }
                }
                return rtn.AsEnumerable();
            });
        }

        public override Task<AVIMConversation> FirstAsync(CancellationToken cancellationToken)
        {
            return this.FirstOrDefaultAsync();
        }

        public override Task<AVIMConversation> FirstOrDefaultAsync(CancellationToken cancellationToken)
        {
            var firstQuery = this.Limit(1);
            return firstQuery.FindAsync().OnSuccess(t =>
            {
                return t.Result.FirstOrDefault();
            });
        }

        public override Task<AVIMConversation> GetAsync(string objectId, CancellationToken cancellationToken)
        {
            var idQuery = this.WhereEqualTo("objectId", objectId);
            return idQuery.FirstAsync();
        }
    }
}
