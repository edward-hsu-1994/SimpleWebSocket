using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleWebSocket {
    /// <summary>
    /// 為<see cref="IApplicationBuilderExtension.UseWebSockets{Handler}(Microsoft.AspNetCore.Builder.IApplicationBuilder, Microsoft.AspNetCore.Builder.WebSocketOptions)"/>方法所參考的處理類型
    /// </summary>
    public abstract class WebSocketHandler {
        /// <summary>
        /// WebSocket連線狀態改變事件
        /// </summary>
        /// <param name="context">Http通訊內容</param>
        /// <param name="socket">WebSocket物件</param>
        public delegate void WebSocketConnectionEvent(HttpContext context, WebSocket socket);

        /// <summary>
        /// WebSocket接收訊息事件
        /// </summary>
        /// <param name="socket">WebSocket物件</param>
        /// <param name="type">訊息類型</param>
        /// <param name="receiveMessage">接收到的訊息</param>
        public delegate void WebSocketReceivedEvent(WebSocket socket, WebSocketMessageType type, byte[] receiveMessage);

        /// <summary>
        /// WebSocket服務發生例外
        /// </summary>
        /// <param name="socket">WebSocket物件</param>
        /// <param name="exception">例外</param>
        public delegate void WebSocketException(WebSocket socket, Exception exception);

        /// <summary>
        /// WebSocket正在接收訊息事件
        /// </summary>
        /// <param name="socket">WebSocket物件</param>
        /// <param name="type">訊息類型</param>
        /// <param name="receiveMessage">接收到的訊息片段</param>
        /// <param name="endOfMessage">是否接收結束</param>
        public delegate void WebsocketReceivingEvent(WebSocket socket, WebSocketMessageType type, byte[] receiveMessage, bool endOfMessage);

        /// <summary>
        /// 表示Request路徑
        /// </summary>
        public string RequestPath { get; set; }

        /// <summary>
        /// 緩衝區大小
        /// </summary>
        public int BufferSize { get; set; } = 1024 * 4;

        /// <summary>
        /// 子通訊協定
        /// </summary>
        public string SubProtocol { get; private set; } = null;

        /// <summary>
        /// Handler建構式，您必須至少聲明Request路徑
        /// </summary>
        /// <param name="requestPath">Request路徑</param>
        /// <param name="subProtocol">子通訊協定</param>
        public WebSocketHandler(string requestPath, string subProtocol = null) {
            this.RequestPath = requestPath;
            this.SubProtocol = subProtocol;
        }

        /// <summary>
        /// Handler進入點
        /// </summary>
        /// <param name="context">Http通訊內容</param>
        /// <param name="next">下一個連線工作</param>
        /// <returns></returns>
        internal async Task Start(HttpContext context, Func<Task> next) {
            //如果不是WebSocket發出的請求，則由其他路由處理
            if (!context.WebSockets.IsWebSocketRequest) {
                await next(); return;
            }

            //根據檢查決定是否允許連線
            if (AcceptConditions(context)) {
                //通知允許連線
                OnAcceptConnected?.Invoke(context);

                var socket = await context.WebSockets.AcceptWebSocketAsync(SubProtocol);

                //轉發監聽
                await Listen(context, socket);
            } else {
                //通知拒絕連線
                OnDenyConnected?.Invoke(context);
            }
        }

        /// <summary>
        /// 允許WebSocket連線之條件
        /// </summary>
        /// <param name="Context">Http通訊內容</param>
        /// <returns>是否允許客戶端連線</returns>
        protected abstract bool AcceptConditions(HttpContext Context);

        /// <summary>
        /// 當WebSocket開啟連線時
        /// </summary>
        protected event WebSocketConnectionEvent OnConnected;

        /// <summary>
        /// 當WebSocket關閉連線時
        /// </summary>
        protected event WebSocketConnectionEvent OnDisconnected;

        /// <summary>
        /// 當WebSocket接收到訊息
        /// </summary>
        protected event WebSocketReceivedEvent OnReceived;

        /// <summary>
        /// 當WebSocket服務發生例外
        /// </summary>
        protected event WebSocketException OnException;

        /// <summary>
        /// 當WebSocket正在接收訊息
        /// </summary>
        protected event WebsocketReceivingEvent OnReceiving;

        /// <summary>
        /// 當符合連線條件時觸發
        /// </summary>
        protected event Action<HttpContext> OnAcceptConnected;

        /// <summary>
        /// 當不符合連線條件時觸發
        /// </summary>
        protected event Action<HttpContext> OnDenyConnected;

        /// <summary>
        /// WebSocket監聽主程序
        /// </summary>
        /// <param name="context">Http通訊內容</param>
        /// <param name="socket">WebSocket物件</param>
        protected async Task Listen(HttpContext context, WebSocket socket) {
            OnConnected?.Invoke(context, socket);
            Exception exception = null;

            //監聽迴圈
            while (socket.State == WebSocketState.Open) {
                List<byte> receiveData = new List<byte>();
                WebSocketReceiveResult receiveResult = null;

                //分段存取迴圈
                do {
                    //緩衝區
                    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[BufferSize]);

                    try {
                        //接收資料
                        receiveResult = await socket.ReceiveAsync(buffer, CancellationToken.None);
                    } catch (Exception e) {
                        exception = e;
                        break;
                    }
                    byte[] rawData = buffer.Array.Take(receiveResult.Count).ToArray();

                    OnReceiving?.Invoke(
                        socket,
                        receiveResult.MessageType,
                        rawData,
                        receiveResult.EndOfMessage);

                    receiveData.AddRange(rawData);
                } while (!receiveResult.EndOfMessage);

                OnReceived?.Invoke(socket, receiveResult.MessageType, receiveData.ToArray());

                //檢查是否關閉連線，如關閉則跳脫循環監聽
                if (exception != null ||
                    receiveResult.CloseStatus.HasValue ||
                    socket.State != WebSocketState.Open) break;
            }

            if (exception != null) OnException?.Invoke(socket, exception);

            OnDisconnected?.Invoke(context, socket);
        }
    }
}
