using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;
using UnityEngine;


namespace LeanCloud.Realtime.Internal
{

    /// <summary>
    /// LeanCluod Unity Realtime SDK 内置默认的 WebSocketClient
    /// 开发者可以在初始化的时候指定自定义的 WebSocketClient
    /// </summary>
    public class DefaultWebSocketClient : IWebSocketClient
    {
        WebSocket ws;
        public bool IsOpen
        {
            get
            {
                return ws.IsAlive;
            }
        }

        public event Action OnClosed;
        public event Action<string> OnError;
        public event Action<string> OnLog;
        public event Action<string> OnMessage;
        public event Action OnOpened;

        public void Close()
        {
            ws.CloseAsync();
        }

        public void Open(string url, string protocol = null)
        {
            Debug.Log(url + " connecting...");
            ws = new WebSocket(url);
            ws.OnOpen += Ws_OnOpen;
            ws.OnMessage += Ws_OnMessage;
            ws.OnClose += Ws_OnClose;
            ws.ConnectAsync();
        }

        private void Ws_OnClose(object sender, CloseEventArgs e)
        {
            this.OnClosed();
        }

        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            this.OnMessage(e.Data);
        }

        private void Ws_OnOpen(object sender, EventArgs e)
        {
            Debug.Log("connected.");
            this.OnOpened();
        }

        public void Send(string message)
        {
            Debug.Log(message+" sending...");
            ws.SendAsync(message, (b) =>
            {
                if(b) Debug.Log(message + " sent.");
            });
        }
    }
}
