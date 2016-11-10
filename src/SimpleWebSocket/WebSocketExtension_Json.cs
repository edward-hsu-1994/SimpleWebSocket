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
    public static class WebSocketExtension_Json {
        /// <summary>
        /// 非同步送出Json
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">Json內容</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        public static async Task SendJsonAsync(this WebSocket obj, JToken data, Encoding encoding, CancellationToken cancellationToken, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            await obj.SendTextAsync(data.ToString(), encoding, cancellationToken, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步送出Json
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">Json內容</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        public static async Task SendJsonAsync(this WebSocket obj, JToken data, Encoding encoding, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            await obj.SendTextAsync(data.ToString(), encoding, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步送出Json(UTF8)
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">Json內容</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        public static async Task SendJsonAsync(this WebSocket obj, JToken data, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            await obj.SendTextAsync(data.ToString(), bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步接收Json資料
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的Json</returns>
        public static async Task<JToken> ReceiveJsonAsync(this WebSocket obj, Encoding encoding, CancellationToken cancellationToken, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return JToken.Parse(encoding.GetString(await obj.ReceiveAsync(cancellationToken, bufferSize, millisecondsTimeout)));
        }

        /// <summary>
        /// 非同步接收Json資料(UTF8)
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的Json</returns>
        public static async Task<JToken> ReceiveJsonAsync(this WebSocket obj, CancellationToken cancellationToken, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return await obj.ReceiveJsonAsync(Encoding.UTF8, cancellationToken, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步接收Json資料
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的Json</returns>
        public static async Task<JToken> ReceiveJsonAsync(this WebSocket obj, Encoding encoding, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return await obj.ReceiveJsonAsync(encoding, CancellationToken.None, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步接收Json資料(UTF8)
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的Json</returns>
        public static async Task<JToken> ReceiveJsonAsync(this WebSocket obj, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return await obj.ReceiveJsonAsync(CancellationToken.None, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步送出Json並等候接收
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的Json</returns>
        public static async Task<JToken> SendAndReceiveJsonAsync(this WebSocket obj, JToken data, Encoding encoding, CancellationToken cancellationToken, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            JToken result = null;
            var notTimeout = await TaskFactory.LimitedTask(async () => {
                await obj.SendJsonAsync(data, encoding, cancellationToken, bufferSize);
                result = await obj.ReceiveJsonAsync(cancellationToken, bufferSize);
            }, millisecondsTimeout);
            if (!notTimeout) throw new TimeoutException();
            return result;
        }

        /// <summary>
        /// 非同步送出Json並等候接收
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="encoding">文字編碼(</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的Json</returns>
        public static async Task<JToken> SendAndReceiveJsonAsync(this WebSocket obj, JToken data, Encoding encoding, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return await obj.SendAndReceiveJsonAsync(data, encoding, CancellationToken.None, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步送出Json(UTF8)並等候接收
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的Json</returns>
        public static async Task<JToken> SendAndReceiveJsonAsync(this WebSocket obj, JToken data, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return await obj.SendAndReceiveJsonAsync(data, Encoding.UTF8, bufferSize, millisecondsTimeout);
        }
    }
}
