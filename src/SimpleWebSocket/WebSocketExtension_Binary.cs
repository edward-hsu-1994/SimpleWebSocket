using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleWebSocket {
    /// <summary>
    /// 針對<see cref="WebSocket"/>類型的擴充方法
    /// </summary>
    public static class WebSocketExtension_Binary {
        /// <summary>
        /// 透過WebSocket以非同步的方式傳送資料
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="buffer">要透過連線傳送的緩衝區</param>
        /// <param name="messageType">表示應用程式正在傳送二進位或文字訊息</param>
        /// <param name="endOfMessage">指示「緩衝區」中的資料是否為訊息的最後一部分</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        public static async Task SendAsync(this WebSocket obj, byte[] buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            if (bufferSize < 1) throw new ArgumentOutOfRangeException(nameof(bufferSize), "緩衝區大小不該小於1");
            for (int i = 0; i < buffer.Length; i += bufferSize) {
                //訊息結束檢查
                bool end = false;
                bool eof = i + bufferSize >= buffer.Length;
                if (eof) end = endOfMessage;

                ArraySegment<byte> segments = new ArraySegment<byte>(buffer, i, end ? buffer.Length - i : bufferSize);
                bool notTimeout = await TaskFactory.LimitedTask(async () => {
                    await obj.SendAsync(segments, messageType, end, cancellationToken);
                }, millisecondsTimeout);
                if (!notTimeout) throw new TimeoutException();
            }
        }

        /// <summary>
        /// 透過WebSocket以非同步的方式傳送資料
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="buffer">要透過連線傳送的緩衝區</param>
        /// <param name="messageType">表示應用程式正在傳送二進位或文字訊息</param>
        /// <param name="endOfMessage">指示「緩衝區」中的資料是否為訊息的最後一部分</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        public static async Task SendAsync(this WebSocket obj, byte[] buffer, WebSocketMessageType messageType, bool endOfMessage, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            await obj.SendAsync(buffer, messageType, endOfMessage, CancellationToken.None, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步接收資料
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的位元組</returns>
        public static async Task<byte[]> ReceiveAsync(this WebSocket obj, CancellationToken cancellationToken, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            List<byte> receiveData = new List<byte>();
            WebSocketReceiveResult receiveResult = null;

            //分段存取迴圈
            do {
                //緩衝區
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[bufferSize]);

                try {
                    //接收資料
                    var notTimeout = await TaskFactory.LimitedTask(async () => {
                        receiveResult = await obj.ReceiveAsync(buffer, cancellationToken);
                    }, millisecondsTimeout);
                    if (!notTimeout) throw new TimeoutException();
                } catch (Exception e) {
                    break;
                }
                byte[] rawData = buffer.Array.Take(receiveResult.Count).ToArray();

                receiveData.AddRange(rawData);
            } while (!receiveResult.EndOfMessage);

            return receiveData.ToArray();
        }

        /// <summary>
        /// 非同步接收資料
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的位元組</returns>
        public static async Task<byte[]> ReceiveAsync(this WebSocket obj, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return await obj.ReceiveAsync(CancellationToken.None, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 透過WebSocket以非同步的方式傳送資料並等候接收
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="buffer">要透過連線傳送的緩衝區</param>
        /// <param name="messageType">表示應用程式正在傳送二進位或文字訊息</param>
        /// <param name="endOfMessage">指示「緩衝區」中的資料是否為訊息的最後一部分</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的位元組</returns>
        public static async Task<byte[]> SendAndReceiveAsync(this WebSocket obj, byte[] buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            byte[] result = null;
            var notTimeout = await TaskFactory.LimitedTask(async () => {
                await obj.SendAsync(buffer, messageType, endOfMessage, cancellationToken, bufferSize);
                result = await obj.ReceiveAsync(cancellationToken, bufferSize);
            }, millisecondsTimeout);
            if (!notTimeout) throw new TimeoutException();
            return result;
        }

        /// <summary>
        /// 透過WebSocket以非同步的方式傳送資料並等候接收
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="buffer">要透過連線傳送的緩衝區</param>
        /// <param name="messageType">表示應用程式正在傳送二進位或文字訊息</param>
        /// <param name="endOfMessage">指示「緩衝區」中的資料是否為訊息的最後一部分</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的位元組</returns>
        public static async Task<byte[]> SendAndReceiveAsync(this WebSocket obj, byte[] buffer, WebSocketMessageType messageType, bool endOfMessage, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return await obj.SendAndReceiveAsync(buffer, messageType, endOfMessage, CancellationToken.None, bufferSize, millisecondsTimeout);
        }

    }
}
