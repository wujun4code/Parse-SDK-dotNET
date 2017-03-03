using LeanCloud.Realtime.Internal;
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
            return rtn;
        }
        internal AVIMCommand GenerateConversationCommand()
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

        public override Task<int> CountAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<AVIMConversation>> FindAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<AVIMConversation> FirstAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        override Task<AVIMConversation> FirstOrDefaultAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<AVIMConversation> GetAsync(string objectId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
