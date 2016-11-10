using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        #region Send
        /// <summary>
        /// 透過WebSocket以非同步的方式傳送資料
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="buffer">要透過連線傳送的緩衝區</param>
        /// <param name="messageType">表示應用程式正在傳送二進位或文字訊息</param>
        /// <param name="endOfMessage">指示「緩衝區」中的資料是否為訊息的最後一部分</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        public static async Task SendAsync(this WebSocket obj, byte[] buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken, int bufferSize = 1024 * 4) {
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
        public static async Task SendAsync(this WebSocket obj, byte[] buffer, WebSocketMessageType messageType, bool endOfMessage, int bufferSize = 1024 * 4) {
            await obj.SendAsync(buffer, messageType, endOfMessage, CancellationToken.None, bufferSize);
        }
        
        /// <summary>
        /// 非同步送出文字
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        public static async Task SendTextAsync(this WebSocket obj, string data, Encoding encoding, CancellationToken cancellationToken, int bufferSize = 1024 * 4) {
            await obj.SendAsync(encoding.GetBytes(data), WebSocketMessageType.Text, true, cancellationToken,  bufferSize);
        }

        /// <summary>
        /// 非同步送出文字
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="bufferSize">緩衝區大小</param>
        public static async Task SendTextAsync(this WebSocket obj, string data, Encoding encoding, int bufferSize = 1024 * 4) {
            await obj.SendAsync(encoding.GetBytes(data), WebSocketMessageType.Text, true, CancellationToken.None, bufferSize);
        }

        /// <summary>
        /// 非同步送出文字(UTF8)
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="bufferSize">緩衝區大小</param>
        public static async Task SendTextAsync(this WebSocket obj, string data, int bufferSize = 1024 * 4) {
            await obj.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text, true, CancellationToken.None, bufferSize);
        }
                
        /// <summary>
        /// 非同步送出Json
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">Json內容</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        public static async Task SendJsonAsync(this WebSocket obj, JToken data, Encoding encoding, CancellationToken cancellationToken, int bufferSize = 1024 * 4) {
            await obj.SendTextAsync(data.ToString(), encoding, cancellationToken, bufferSize);
        }

        /// <summary>
        /// 非同步送出Json
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">Json內容</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="bufferSize">緩衝區大小</param>
        public static async Task SendJsonAsync(this WebSocket obj, JToken data, Encoding encoding, int bufferSize = 1024 * 4) {
            await obj.SendTextAsync(data.ToString(), encoding, bufferSize);
        }

        /// <summary>
        /// 非同步送出Json(UTF8)
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">Json內容</param>
        /// <param name="bufferSize">緩衝區大小</param>
        public static async Task SendJsonAsync(this WebSocket obj, string data, int bufferSize = 1024 * 4) {
            await obj.SendTextAsync(data.ToString(), bufferSize);
        }
        #endregion

        #region Receive
        /// <summary>
        /// 非同步接收資料
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <returns>接收到的位元組</returns>
        public static async Task<byte[]> ReceiveAsync(this WebSocket obj, CancellationToken cancellationToken, int bufferSize = 1024 * 4) {
            List<byte> receiveData = new List<byte>();
            WebSocketReceiveResult receiveResult = null;

            //分段存取迴圈
            do {
                //緩衝區
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[bufferSize]);

                try {
                    //接收資料
                    receiveResult = await obj.ReceiveAsync(buffer, cancellationToken);
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
        /// <returns>接收到的位元組</returns>
        public static async Task<byte[]> ReceiveAsync(this WebSocket obj, int bufferSize = 1024 * 4) {
            return await obj.ReceiveAsync(CancellationToken.None, bufferSize);
        }

        /// <summary>
        /// 非同步接收字串資料
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <returns>接收到的字串</returns>
        public static async Task<string> ReceiveTextAsync(this WebSocket obj, Encoding encoding, CancellationToken cancellationToken, int bufferSize = 1024 * 4) {
            return encoding.GetString(await obj.ReceiveAsync(cancellationToken, bufferSize));
        }

        /// <summary>
        /// 非同步接收字串資料(UTF8)
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <returns>接收到的字串</returns>
        public static async Task<string> ReceiveTextAsync(this WebSocket obj, CancellationToken cancellationToken, int bufferSize = 1024 * 4) {
            return await obj.ReceiveTextAsync(Encoding.UTF8, cancellationToken, bufferSize);
        }
        
        /// <summary>
        /// 非同步接收字串資料
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <returns>接收到的字串</returns>
        public static async Task<string> ReceiveTextAsync(this WebSocket obj, Encoding encoding, int bufferSize = 1024 * 4) {
            return await obj.ReceiveTextAsync(encoding, CancellationToken.None, bufferSize);
        }

        /// <summary>
        /// 非同步接收字串資料(UTF8)
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <returns>接收到的字串</returns>
        public static async Task<string> ReceiveTextAsync(this WebSocket obj, int bufferSize = 1024 * 4) {
            return await obj.ReceiveTextAsync(CancellationToken.None, bufferSize);
        }
        
        /// <summary>
        /// 非同步接收Json資料
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <returns>接收到的Json</returns>
        public static async Task<JToken> ReceiveJsonAsync(this WebSocket obj, Encoding encoding, CancellationToken cancellationToken, int bufferSize = 1024 * 4) {
            return JToken.Parse(encoding.GetString(await obj.ReceiveAsync(cancellationToken, bufferSize)));
        }

        /// <summary>
        /// 非同步接收Json資料(UTF8)
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <returns>接收到的Json</returns>
        public static async Task<JToken> ReceiveJsonAsync(this WebSocket obj, CancellationToken cancellationToken, int bufferSize = 1024 * 4) {
            return await obj.ReceiveJsonAsync(Encoding.UTF8, cancellationToken, bufferSize);
        }

        /// <summary>
        /// 非同步接收Json資料
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <returns>接收到的Json</returns>
        public static async Task<JToken> ReceiveJsonAsync(this WebSocket obj, Encoding encoding, int bufferSize = 1024 * 4) {
            return await obj.ReceiveJsonAsync(encoding, CancellationToken.None, bufferSize);
        }

        /// <summary>
        /// 非同步接收Json資料(UTF8)
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <returns>接收到的Json</returns>
        public static async Task<JToken> ReceiveJsonAsync(this WebSocket obj, int bufferSize = 1024 * 4) {
            return await obj.ReceiveJsonAsync(CancellationToken.None, bufferSize);
        }
        #endregion

        #region SendAndWaitReceive
        /// <summary>
        /// 透過WebSocket以非同步的方式傳送資料並等候接收
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="buffer">要透過連線傳送的緩衝區</param>
        /// <param name="messageType">表示應用程式正在傳送二進位或文字訊息</param>
        /// <param name="endOfMessage">指示「緩衝區」中的資料是否為訊息的最後一部分</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <returns>接收到的位元組</returns>
        public static async Task<byte[]> SendAndReceiveAsync(this WebSocket obj, byte[] buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken, int bufferSize = 1024 * 4) {
            await obj.SendAsync(buffer, messageType, endOfMessage, cancellationToken, buffer);
            return await obj.ReceiveAsync(cancellationToken, bufferSize);
        }

        /// <summary>
        /// 透過WebSocket以非同步的方式傳送資料並等候接收
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="buffer">要透過連線傳送的緩衝區</param>
        /// <param name="messageType">表示應用程式正在傳送二進位或文字訊息</param>
        /// <param name="endOfMessage">指示「緩衝區」中的資料是否為訊息的最後一部分</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <returns>接收到的位元組</returns>
        public static async Task SendAndReceiveAsync(this WebSocket obj, byte[] buffer, WebSocketMessageType messageType, bool endOfMessage, int bufferSize = 1024 * 4) {
            await obj.SendAsync(buffer, messageType, endOfMessage, CancellationToken.None, bufferSize);
        }

        /// <summary>
        /// 非同步送出文字並等候接收
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <returns>接收到的字串</returns>
        public static async Task<string> SendAndReceiveTextAsync(this WebSocket obj, string data, Encoding encoding, CancellationToken cancellationToken, int bufferSize = 1024 * 4) {
            await obj.SendAsync(encoding.GetBytes(data), WebSocketMessageType.Text, true, cancellationToken, bufferSize);
            return await obj.ReceiveTextAsync(encoding, cancellationToken, bufferSize);
        }

        /// <summary>
        /// 非同步送出文字
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <returns>接收到的字串</returns>
        public static async Task<string> SendAndReceiveTextAsync(this WebSocket obj, string data, Encoding encoding, int bufferSize = 1024 * 4) {
            return await obj.SendAndReceiveTextAsync(data, encoding, CancellationToken.None, bufferSize);
        }

        /// <summary>
        /// 非同步送出文字(UTF8)
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="bufferSize">緩衝區大小</param>
        public static async Task<string> SendAndReceiveTextAsync(this WebSocket obj, string data, int bufferSize = 1024 * 4) {
            return await obj.SendAndReceiveTextAsync(data, Encoding.UTF8, bufferSize);
        }
        
        /// <summary>
        /// 非同步送出Json並等候接收
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <returns>接收到的Json</returns>
        public static async Task<JToken> SendAndReceiveJsonAsync(this WebSocket obj, string data, Encoding encoding, CancellationToken cancellationToken, int bufferSize = 1024 * 4) {
            await obj.SendJsonAsync(data, encoding, cancellationToken, bufferSize);
            return await obj.ReceiveJsonAsync(encoding, cancellationToken, bufferSize);
        }

        /// <summary>
        /// 非同步送出Json
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="encoding">文字編碼(</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <returns>接收到的Json</returns>
        public static async Task<JToken> SendAndReceiveJsonAsync(this WebSocket obj, string data, Encoding encoding, int bufferSize = 1024 * 4) {
            return await obj.SendAndReceiveJsonAsync(data, encoding, CancellationToken.None, bufferSize);
        }

        /// <summary>
        /// 非同步送出Json(UTF8)
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <returns>接收到的Json</returns>
        public static async Task<JToken> SendAndReceiveJsonAsync(this WebSocket obj, string data, int bufferSize = 1024 * 4) {
            return await obj.SendAndReceiveJsonAsync(data, Encoding.UTF8, bufferSize);
        }
        #endregion
    }
}