using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanCloud.Realtime
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class AVIMMessageClassNameAttribute: Attribute
    {
        public AVIMMessageClassNameAttribute(int typeEnumIntValue)
        {
            this.TypeEnumIntValue = typeEnumIntValue;
        }
        public int TypeEnumIntValue { get; private set; }

    }
}
