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
        public delegate void WebsocketReceivedEvent(WebSocket socket, WebSocketMessageType type, byte[] receiveMessage);

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
                //轉發監聽
                Listen(context, await context.WebSockets.AcceptWebSocketAsync(this.SubProtocol));
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
        protected event WebsocketReceivedEvent OnReceived;

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
        protected async void Listen(HttpContext context, WebSocket socket) {
            OnConnected?.Invoke(context, socket);

            while (true) {
                bool ReceiveComplete = true;
                WebSocketReceiveResult ReceiveResult;
                List<byte> ReceiveData = new List<byte>();

                //循環接收資料以防資料大於緩衝區大小時分段傳輸
                do {
                    //建立緩衝區
                    byte[] Buffer = new byte[BufferSize];

                    //接收資料
                    ReceiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(Buffer), CancellationToken.None);

                    //乾淨資料
                    byte[] ClearData = new byte[ReceiveResult.Count];

                    //複製本次傳輸範圍資料
                    Array.Copy(Buffer, ClearData, ReceiveResult.Count);
                                        
                    OnReceiving?.Invoke(socket, ReceiveResult.MessageType, ClearData, ReceiveResult.EndOfMessage);

                    //存入接收資料集合
                    ReceiveData.AddRange(ClearData);

                    //檢查是否接收完成
                    ReceiveComplete = !ReceiveResult.EndOfMessage;
                } while (ReceiveComplete);

                //檢查是否關閉連線，如關閉則跳脫循環監聽
                if (ReceiveResult.CloseStatus.HasValue) break;

                OnReceived?.Invoke(socket, ReceiveResult.MessageType, ReceiveData.ToArray());
            };

            OnDisconnected?.Invoke(context, socket);
        }
    }
}
