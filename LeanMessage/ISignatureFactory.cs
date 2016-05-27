using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanMessage
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISignatureFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        Task<AVIMSignature> CreateConnectSignature(string clientId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="clientId"></param>
        /// <param name="targetIds"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<AVIMSignature> CreateConversationSignature(string conversationId, string clientId, IList<string> targetIds, string action);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="targetIds"></param>
        /// <returns></returns>
        Task<AVIMSignature> CreateStartConversationSignature(string clientId, IList<string> targetIds);


        /// <summary>        
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        Task<AVIMSignature> CreateQueryHistorySignature(string clientId, string conversationId);
    }

    internal class DefaultSignatureFactory : ISignatureFactory
    {
        public Task<AVIMSignature> CreateConnectSignature(string clientId)
        {
            return Task.FromResult<AVIMSignature>(null);
        }

        public Task<AVIMSignature> CreateConversationSignature(string conversationId, string clientId, IList<string> targetIds, string action)
        {
            return Task.FromResult<AVIMSignature>(null);
        }

        public Task<AVIMSignature> CreateQueryHistorySignature(string clientId, string conversationId)
        {
            return Task.FromResult<AVIMSignature>(null);
        }

        public Task<AVIMSignature> CreateStartConversationSignature(string clientId, IList<string> targetIds)
        {
            return Task.FromResult<AVIMSignature>(null);
        }
    }
}
