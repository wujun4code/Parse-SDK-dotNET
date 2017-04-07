using LeanCloud.Storage.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud.Realtime.Internal
{
    internal class MessageSubclassingController : IMessageSubclassingController
    {
        private static readonly int defaultTypeIntValue = 0;

        private readonly ReaderWriterLockSlim mutex;
        private readonly IDictionary<int, MessageSubclassInfo> registeredSubclasses;

        private Dictionary<int, Action> registerActions;

        public MessageSubclassingController()
        {
            mutex = new ReaderWriterLockSlim();
            registeredSubclasses = new Dictionary<int, MessageSubclassInfo>();
            registerActions = new Dictionary<int, Action>();
        }

        public void AddRegisterHook(Type t, Action action)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, string> GetPropertyMappings(int typeEnumIntValue)
        {
            MessageSubclassInfo info = null;
            mutex.EnterReadLock();
            registeredSubclasses.TryGetValue(typeEnumIntValue, out info);
            if (info == null)
            {
                registeredSubclasses.TryGetValue(defaultTypeIntValue, out info);
            }
            mutex.ExitReadLock();

            return info.PropertyMappings;
        }

        public Type GetType(int typeEnumIntValue)
        {
            MessageSubclassInfo info = null;
            mutex.EnterReadLock();
            registeredSubclasses.TryGetValue(typeEnumIntValue, out info);
            mutex.ExitReadLock();

            return info != null
              ? info.TypeInfo.AsType()
              : null;
        }

        public int GetTypeEnumIntValue(Type type)
        {
            return type == typeof(AVIMMessage)? 0: GetTypeEnumIntValue(type.GetTypeInfo());
        }
        internal static int GetTypeEnumIntValue(TypeInfo type)
        {
            var attribute = type.GetCustomAttribute<AVIMMessageClassNameAttribute>();
            return attribute != null ? attribute.TypeEnumIntValue : 0;
        }
        public AVIMMessage Instantiate(int typeEnumIntValue)
        {
            MessageSubclassInfo info = null;

            mutex.EnterReadLock();
            registeredSubclasses.TryGetValue(typeEnumIntValue, out info);
            mutex.ExitReadLock();

            return info != null
              ? info.Instantiate()
              : new AVIMMessage();
        }

        public bool IsTypeValid(int typeEnumIntValue, Type type)
        {
            MessageSubclassInfo subclassInfo = null;

            mutex.EnterReadLock();
            registeredSubclasses.TryGetValue(typeEnumIntValue, out subclassInfo);
            mutex.ExitReadLock();

            return subclassInfo == null
              ? type == typeof(AVIMMessage)
              : subclassInfo.TypeInfo == type.GetTypeInfo();
        }

        public void RegisterSubclass(Type type)
        {
            TypeInfo typeInfo = type.GetTypeInfo();
            if (!typeof(AVIMMessage).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                throw new ArgumentException("Cannot register a type that is not a subclass of AVIMMessage");
            }

            int typeEnumIntValue = GetTypeEnumIntValue(type);

            try
            {
                // Perform this as a single independent transaction, so we can never get into an
                // intermediate state where we *theoretically* register the wrong class due to a
                // TOCTTOU bug.
                mutex.EnterWriteLock();

                MessageSubclassInfo previousInfo = null;
                if (registeredSubclasses.TryGetValue(typeEnumIntValue, out previousInfo))
                {
                    if (typeInfo.IsAssignableFrom(previousInfo.TypeInfo))
                    {
                        // Previous subclass is more specific or equal to the current type, do nothing.
                        return;
                    }
                    else if (previousInfo.TypeInfo.IsAssignableFrom(typeInfo))
                    {
                        // Previous subclass is parent of new child, fallthrough and actually register
                        // this class.
                        /* Do nothing */
                    }
                    else
                    {
                        throw new ArgumentException(
                          "Tried to register both " + previousInfo.TypeInfo.FullName + " and " + typeInfo.FullName +
                          " as the AVIMMessage subclass of " + typeEnumIntValue + ". Cannot determine the right class " +
                          "to use because neither inherits from the other."
                        );
                    }
                }

                ConstructorInfo constructor = type.FindConstructor();
                if (constructor == null)
                {
                    throw new ArgumentException("Cannot register a type that does not implement the default constructor!");
                }

                registeredSubclasses[typeEnumIntValue] = new MessageSubclassInfo(type, constructor);
            }
            finally
            {
                mutex.ExitWriteLock();
            }

            Action toPerform;

            mutex.EnterReadLock();
            registerActions.TryGetValue(typeEnumIntValue, out toPerform);
            mutex.ExitReadLock();

            if (toPerform != null)
            {
                toPerform();
            }
        }

        public void UnregisterSubclass(Type t)
        {
            mutex.EnterWriteLock();
            registeredSubclasses.Remove(GetTypeEnumIntValue(t));
            mutex.ExitWriteLock();
        }
    }
}
