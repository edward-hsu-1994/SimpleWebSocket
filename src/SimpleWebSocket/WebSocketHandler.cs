using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleWebSocket {
    /// <summary>
    /// 為<see cref="IApplicationBuilderExtension.UseWebSockets{Handler}(Microsoft.AspNet.Builder.IApplicationBuilder, Microsoft.AspNet.WebSockets.Server.WebSocketOptions)"/>方法所參考的處理類型
    /// </summary>
    public abstract class WebSocketHandler {
        /// <summary>
        /// WebSocket連線狀態改變事件
        /// </summary>
        /// <param name="Context">Http通訊內容</param>
        /// <param name="Socket">WebSocket物件</param>
        public delegate void WebSocketConnectionEvent(HttpContext Context, WebSocket Socket);

        /// <summary>
        /// WebSocket接收訊息事件
        /// </summary>
        /// <param name="Socket">WebSocket物件</param>
        /// <param name="Type">訊息類型</param>
        /// <param name="ReceiveMessage">接收到的訊息</param>
        public delegate void WebsocketReceiveEvent(WebSocket Socket, WebSocketMessageType Type, byte[] ReceiveMessage);

        /// <summary>
        /// 表示Request路徑
        /// </summary>
        public string RequestPath { get; set; }

        /// <summary>
        /// 緩衝區大小
        /// </summary>
        public int BufferSize { get; set; } = 1024 * 4;

        /// <summary>
        /// Handler建構式，您必須至少聲明Request路徑
        /// </summary>
        /// <param name="RequestPath">Request路徑</param>
        /// <param name="SubProtocol">子通訊協定</param>
        public WebSocketHandler(string RequestPath, string SubProtocol = null) {
            this.RequestPath = RequestPath;
        }

        /// <summary>
        /// Handler進入點
        /// </summary>
        /// <param name="Context">Http通訊內容</param>
        /// <param name="Next">下一個連線工作</param>
        /// <returns></returns>
        internal async Task Start(HttpContext Context, Func<Task> Next) {
            //如果不是WebSocket發出的請求，則由其他路由處理
            if (!Context.WebSockets.IsWebSocketRequest) {
                await Next(); return;
            }

            //根據檢查決定是否允許連線
            if (AcceptConditions(Context)) {
                //通知允許連線
                OnAcceptConnected?.Invoke(Context, null);
                //轉發監聽
                Listen(Context, await Context.WebSockets.AcceptWebSocketAsync());
            } else {
                //通知拒絕連線
                OnDenyConnected?.Invoke(Context, null);
            }
        }

        /// <summary>
        /// 允許WebSocket連線之條件
        /// </summary>
        /// <param name="Context">Http通訊內容</param>
        /// <returns></returns>
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
        protected event WebsocketReceiveEvent OnReceive;

        /// <summary>
        /// 當符合連線條件時觸發
        /// </summary>
        protected event WebSocketConnectionEvent OnAcceptConnected;

        /// <summary>
        /// 當不符合連線條件時觸發
        /// </summary>
        protected event WebSocketConnectionEvent OnDenyConnected;

        /// <summary>
        /// WebSocket監聽主程序
        /// </summary>
        /// <param name="Context">Http通訊內容</param>
        /// <param name="Socket">WebSocket物件</param>
        protected async void Listen(HttpContext Context, WebSocket Socket) {
            OnConnected?.Invoke(Context, Socket);

            while (true) {
                bool ReceiveComplete = true;
                WebSocketReceiveResult ReceiveResult;
                List<byte> ReceiveData = new List<byte>();

                //循環接收資料以防資料大於緩衝區大小時分段傳輸
                do {
                    //建立緩衝區
                    byte[] Buffer = new byte[BufferSize];

                    //接收資料
                    ReceiveResult = await Socket.ReceiveAsync(new ArraySegment<byte>(Buffer), CancellationToken.None);

                    //乾淨資料
                    byte[] ClearData = new byte[ReceiveResult.Count];

                    //複製本次傳輸範圍資料
                    Array.Copy(Buffer, ClearData, ReceiveResult.Count);

                    //存入接收資料集合
                    ReceiveData.AddRange(ClearData);

                    //檢查是否接收完成
                    ReceiveComplete = !ReceiveResult.EndOfMessage;
                } while (ReceiveComplete);

                //檢查是否關閉連線，如關閉則跳脫循環監聽
                if (ReceiveResult.CloseStatus.HasValue) break;

                OnReceive?.Invoke(Socket, ReceiveResult.MessageType, ReceiveData.ToArray());
            };

            OnDisconnected?.Invoke(Context, Socket);
        }
    }
}
