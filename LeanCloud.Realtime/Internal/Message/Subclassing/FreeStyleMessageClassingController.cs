using LeanCloud.Storage.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace LeanCloud.Realtime.Internal
{
    internal class FreeStyleMessageClassingController : IFreeStyleMessageClassingController
    {
        private readonly IList<FreeStyleMessageClassInfo> registeredInterfaces;
        private readonly ReaderWriterLockSlim mutex;
        public FreeStyleMessageClassingController()
        {
            mutex = new ReaderWriterLockSlim();
            registeredInterfaces = new List<FreeStyleMessageClassInfo>();
        }
        public Type GetType(IDictionary<string, object> msg)
        {
            throw new NotImplementedException();
        }

        public IAVIMMessage Instantiate(IDictionary<string, object> msg, IDictionary<string, object> buildInData)
        {
            FreeStyleMessageClassInfo info = null;
            mutex.EnterReadLock();
            foreach (var subInterface in registeredInterfaces)
            {
                if (subInterface.Validate(msg))
                {
                    info = subInterface;
                }
            }
            mutex.ExitReadLock();
            var rtn = info != null ? info.Instantiate(msg) : new AVIMMessage();

            if (buildInData.ContainsKey("timestamp"))
            {
                long timestamp = 0;
                if (long.TryParse(buildInData["timestamp"].ToString(), out timestamp))
                {
                    rtn.ServerTimestamp = timestamp;
                }
            }
            if (buildInData.ContainsKey("ackAt"))
            {
                long ackAt = 0;
                if (long.TryParse(buildInData["ackAt"].ToString(), out ackAt))
                {
                    rtn.RcpTimestamp = ackAt;
                }
            }
            if (buildInData.ContainsKey("from"))
            {
                rtn.FromClientId = buildInData["from"].ToString();
            }
            if (buildInData.ContainsKey("msgId"))
            {
                rtn.Id = buildInData["msgId"].ToString();
            }
            if (buildInData.ContainsKey("cid"))
            {
                rtn.ConversationId = buildInData["cid"].ToString();
            }
            if (buildInData.ContainsKey("fromPeerId"))
            {
                rtn.FromClientId = buildInData["fromPeerId"].ToString();
            }
            if (buildInData.ContainsKey("id"))
            {
                rtn.Id = buildInData["id"].ToString();
            }
            rtn.Restore(msg);
            return rtn;
        }

        public bool IsTypeValid(IDictionary<string, object> msg, Type type)
        {
            throw new NotImplementedException();
        }

        public void RegisterSubclass(Type type)
        {
            TypeInfo typeInfo = type.GetTypeInfo();
            if (!typeof(IAVIMMessage).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                throw new ArgumentException("Cannot register a type that is not a implementation of IAVIMMessage");
            }

            try
            {
                // Perform this as a single independent transaction, so we can never get into an
                // intermediate state where we *theoretically* register the wrong class due to a
                // TOCTTOU bug.
                mutex.EnterWriteLock();


                ConstructorInfo constructor = type.FindConstructor();
                if (constructor == null)
                {
                    throw new ArgumentException("Cannot register a type that does not implement the default constructor!");
                }

                registeredInterfaces.Add(new FreeStyleMessageClassInfo(type, constructor));
            }
            finally
            {
                mutex.ExitWriteLock();
            }
        }
    }
}
