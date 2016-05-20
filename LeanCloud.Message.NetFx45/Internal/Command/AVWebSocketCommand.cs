using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanMessage.Internal
{
    internal class AVWebSocketCommand
    {
        private readonly string cmd;
        private readonly string op;
        private readonly string appId;
        private readonly string peerId;
        private AVIMSignature signature;
        private readonly IDictionary<string, object> arguments;

        public AVWebSocketCommand()
        {
            arguments = new Dictionary<string, object>();
        }

        private AVWebSocketCommand(AVWebSocketCommand source,
            string cmd = null,
            string op = null,
            string appId = null,
            string peerId = null,
            IDictionary<string,object> arguments = null,
            AVIMSignature signature = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            this.cmd = source.cmd;
            this.op = source.op;
            this.arguments = source.arguments;
            this.peerId = source.peerId;
            this.appId = source.appId;
            this.signature = source.signature;

            if (cmd != null)
            {
                this.cmd = cmd;
            }
            if (op != null)
            {
                this.op = op;
            }
            if (arguments != null)
            {
                this.arguments = arguments;
            }
            if (peerId != null)
            {
                this.peerId = peerId;
            }
            if (appId != null)
            {
                this.appId = appId;
            }
            if (signature != null)
            {
                this.signature = signature;
            }
        }

        public AVWebSocketCommand Command(string cmd)
        {
            return new AVWebSocketCommand(this, cmd: cmd);
        }
        public AVWebSocketCommand Argument(string key, object value)
        {
            this.arguments[key] = value;
            return new AVWebSocketCommand(this);
        }
    }
}
