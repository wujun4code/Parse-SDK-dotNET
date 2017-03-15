using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanCloud.Realtime.Internal
{
    interface IMessageSubclassingController
    {
        int GetTypeEnumIntValue(Type type);

        Type GetType(int typeEnumIntValue);

        bool IsTypeValid(int typeEnumIntValue, Type type);

        void RegisterSubclass(Type t);
        void UnregisterSubclass(Type t);

        void AddRegisterHook(Type t, Action action);

        AVIMMessage Instantiate(int typeEnumIntValue);
        IDictionary<String, String> GetPropertyMappings(int typeEnumIntValue);
    }
}
