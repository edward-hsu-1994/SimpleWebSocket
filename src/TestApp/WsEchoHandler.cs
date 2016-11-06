using SimpleWebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Net.WebSockets;

namespace TestApp
{
    public class WsEchoHandler : WebSocketHandler {
        static List<WebSocket> socketList = new List<WebSocket>();

        public WsEchoHandler() : base("/api/echo") {
            this.OnReceived += WsEchoHandler_OnReceived;
        }

        private async void WsEchoHandler_OnReceived(System.Net.WebSockets.WebSocket socket, System.Net.WebSockets.WebSocketMessageType type, byte[] receiveMessage) {
            await socket.SendAsync(receiveMessage, type,true, this.BufferSize);
        }


        protected override bool AcceptConditions(HttpContext Context) {
            return true;
        }
    }
}
