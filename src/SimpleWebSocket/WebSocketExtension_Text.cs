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
    public static class WebSocketExtension_Text {
        /// <summary>
        /// 非同步送出文字
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        public static async Task SendTextAsync(this WebSocket obj, string data, Encoding encoding, CancellationToken cancellationToken, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            await obj.SendAsync(encoding.GetBytes(data), WebSocketMessageType.Text, true, cancellationToken, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步送出文字
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        public static async Task SendTextAsync(this WebSocket obj, string data, Encoding encoding, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            await obj.SendAsync(encoding.GetBytes(data), WebSocketMessageType.Text, true, CancellationToken.None, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步送出文字(UTF8)
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        public static async Task SendTextAsync(this WebSocket obj, string data, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            await obj.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text, true, CancellationToken.None, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步接收字串資料
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的字串</returns>
        public static async Task<string> ReceiveTextAsync(this WebSocket obj, Encoding encoding, CancellationToken cancellationToken, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return encoding.GetString(await obj.ReceiveAsync(cancellationToken, bufferSize, millisecondsTimeout));
        }

        /// <summary>
        /// 非同步接收字串資料(UTF8)
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的字串</returns>
        public static async Task<string> ReceiveTextAsync(this WebSocket obj, CancellationToken cancellationToken, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return await obj.ReceiveTextAsync(Encoding.UTF8, cancellationToken, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步接收字串資料
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的字串</returns>
        public static async Task<string> ReceiveTextAsync(this WebSocket obj, Encoding encoding, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return await obj.ReceiveTextAsync(encoding, CancellationToken.None, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步接收字串資料(UTF8)
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的字串</returns>
        public static async Task<string> ReceiveTextAsync(this WebSocket obj, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return await obj.ReceiveTextAsync(CancellationToken.None, bufferSize, millisecondsTimeout);
        }


        /// <summary>
        /// 非同步送出文字並等候接收
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的字串</returns>
        public static async Task<string> SendAndReceiveTextAsync(this WebSocket obj, string data, Encoding encoding, CancellationToken cancellationToken, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            string result = null;
            var notTimeout = await TaskFactory.LimitedTask(async () => {
                await obj.SendTextAsync(data, encoding, cancellationToken, bufferSize);
                result = await obj.ReceiveTextAsync(cancellationToken, bufferSize);
            }, millisecondsTimeout);
            if (!notTimeout) throw new TimeoutException();
            return result;
        }

        /// <summary>
        /// 非同步送出文字並等候接收
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的字串</returns>
        public static async Task<string> SendAndReceiveTextAsync(this WebSocket obj, string data, Encoding encoding, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return await obj.SendAndReceiveTextAsync(data, encoding, CancellationToken.None, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步送出文字(UTF8)並等候接收
        /// </summary>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">文字內容</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的字串</returns>
        public static async Task<string> SendAndReceiveTextAsync(this WebSocket obj, string data, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return await obj.SendAndReceiveTextAsync(data, Encoding.UTF8, bufferSize, millisecondsTimeout);
        }

    }
}
