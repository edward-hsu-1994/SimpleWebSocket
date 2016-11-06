using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleWebSocket {
    /// <summary>
    /// 針對<see cref="WebSocket"/>類型的擴充方法
    /// </summary>
    public static class WebSocketExtension {
        /// <summary>
        /// 非同步送出文字
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="encoding">文字編碼，如為null則為預設(UTF8)</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <returns>字串與原始結果</returns>
        public static async Task SendTextAsync(this WebSocket obj, string data, Encoding encoding = null, int bufferSize = 1024 * 4) {
            encoding = encoding ?? Encoding.UTF8;
            await obj.SendAsync(encoding.GetBytes(data), WebSocketMessageType.Text, true, bufferSize, CancellationToken.None);
        }

        /// <summary>
        /// 透過WebSocket以非同步的方式傳送資料
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="buffer">要透過連線傳送的緩衝區</param>
        /// <param name="messageType">表示應用程式正在傳送二進位或文字訊息</param>
        /// <param name="endOfMessage">指示「緩衝區」中的資料是否為訊息的最後一部分</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        public static async Task SendAsync(this WebSocket obj, byte[] buffer, WebSocketMessageType messageType, bool endOfMessage, int bufferSize, CancellationToken cancellationToken) {
            if (bufferSize < 1) throw new ArgumentOutOfRangeException(nameof(bufferSize), "緩衝區大小不該小於1");
            for (int i = 0; i < buffer.Length; i += bufferSize) {
                //訊息結束檢查
                bool end = false;
                bool eof = i + bufferSize >= buffer.Length;
                if (eof) end = endOfMessage;

                ArraySegment<byte> segments = new ArraySegment<byte>(buffer, i, end ? buffer.Length - i : bufferSize);
                await obj.SendAsync(segments, messageType, end, cancellationToken);
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
        public static async Task SendAsync(this WebSocket obj, byte[] buffer, WebSocketMessageType messageType, bool endOfMessage, int bufferSize) {
            await obj.SendAsync(buffer, messageType, endOfMessage, bufferSize, CancellationToken.None);
        }
    }
}
