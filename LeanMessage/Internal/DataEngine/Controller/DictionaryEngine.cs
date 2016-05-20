using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanMessage.Internal
{
    internal static class DictionaryEngine
    {
        internal static IDictionary<string, object> Merge(this IDictionary<string, object> dataLeft, IDictionary<string, object> dataRight)
        {
            if (dataRight == null)
                return dataLeft;
            foreach (var kv in dataRight)
            {
                if (dataLeft.ContainsKey(kv.Key))
                {
                    dataLeft[kv.Key] = kv.Value;
                }
                else
                {
                    dataLeft.Add(kv);
                }
            }
            return dataLeft;
        }
    }
}
