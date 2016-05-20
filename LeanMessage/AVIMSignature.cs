using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanMessage
{
    public class AVIMSignature
    {
        /// <summary>
        /// 经过 SHA1 以及相关操作参数计算出来的加密字符串
        /// </summary>
        public readonly string signatureContent;

        /// <summary>
        /// 服务端时间戳
        /// </summary>
        public readonly long timestamp;

        /// <summary>
        /// 随机字符串
        /// </summary>
        public readonly string nonce;

        public AVIMSignature(string s,long t,string n)
        {
            this.nonce = n;
            this.signatureContent = s;
            this.timestamp = t;
        }
    }
}
