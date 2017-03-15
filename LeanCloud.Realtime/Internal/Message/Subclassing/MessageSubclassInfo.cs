using LeanCloud.Storage.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LeanCloud.Realtime.Internal
{
    internal class MessageSubclassInfo
    {
        public MessageSubclassInfo(Type type, ConstructorInfo constructor)
        {
            TypeInfo = type.GetTypeInfo();
            TypeEnumIntValue = GetTypeEnumIntValue(TypeInfo);
            Constructor = constructor;
            PropertyMappings = ReflectionHelpers.GetProperties(type)
              .Select(prop => Tuple.Create(prop, prop.GetCustomAttribute<AVIMMessageFieldNameAttribute>(true)))
              .Where(t => t.Item2 != null)
              .Select(t => Tuple.Create(t.Item1, t.Item2.FieldName))
              .ToDictionary(t => t.Item1.Name, t => t.Item2);
        }

        public TypeInfo TypeInfo { get; private set; }
        public int TypeEnumIntValue { get; private set; }
        public IDictionary<String, String> PropertyMappings { get; private set; }
        private ConstructorInfo Constructor { get; set; }

        public AVIMMessage Instantiate()
        {
            return (AVIMMessage)Constructor.Invoke(null);
        }

        internal static int GetTypeEnumIntValue(TypeInfo type)
        {
            var attribute = type.GetCustomAttribute<AVIMMessageClassNameAttribute>();
            return attribute != null ? attribute.TypeEnumIntValue : 0;
        }
    }
}
