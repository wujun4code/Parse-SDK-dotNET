using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeanCloud.Realtime.Internal
{
    interface IFreeStyleMessageClassingController
    {
        bool IsTypeValid(IDictionary<string,object> msg, Type type);
        void RegisterSubclass(Type t);
        IAVIMMessage Instantiate(IDictionary<string, object> msg,IDictionary<string,object> buildInData);
        Type GetType(IDictionary<string, object> msg);
    }
}
