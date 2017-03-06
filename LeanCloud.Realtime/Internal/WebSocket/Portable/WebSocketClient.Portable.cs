using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Websockets;

namespace LeanCloud.Realtime.Internal
{
    /// <summary>
    /// LeanCloud Realtime SDK for .NET Portable 内置默认的 WebSocketClient
    /// </summary>
    public class DefaultWebSocketClient : IWebSocketClient
    {
        internal readonly IWebSocketConnection connection;
        public DefaultWebSocketClient()
        {
            //Websockets.Net.WebsocketConnection.Link();
            connection = WebSocketFactory.Create();
        }

        public event Action OnClosed;
        public event Action<string> OnError;
        public event Action<string> OnLog;

        public event Action OnOpened
        {
            add
            {
                connection.OnOpened += value;
            }
            remove
            {
                connection.OnOpened -= value;
            }
        }

        public event Action<string> OnMessage
        {
            add
            {
                connection.OnMessage += value;
            }
            remove
            {
                connection.OnMessage -= value;
            }
        }

        public bool IsOpen
        {
            get
            {
                return connection.IsOpen;
            }
        }

        public void Close()
        {
            if (connection != null)
            {
                connection.Close();
            }
        }

        public void Open(string url, string protocol = null)
        {
            if (connection != null)
            {
                connection.Open(url, protocol);
            }
        }

        public void Send(string message)
        {
            if (connection != null)
            {
                if (this.IsOpen)
                {
                    connection.Send(message);
                }
            }
        }
    }
}
