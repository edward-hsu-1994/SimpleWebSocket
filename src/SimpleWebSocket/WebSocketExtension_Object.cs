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
    public static class WebSocketExtension_Object {
        /// <summary>
        /// 非同步送出物件
        /// </summary>
        /// <typeparam name="T">類型</typeparam>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">物件內容將轉換為Json</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        public static async Task SendJsonAsync<T>(this WebSocket obj, T data, Encoding encoding, CancellationToken cancellationToken, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            await obj.SendTextAsync(JsonConvert.SerializeObject(data), encoding, cancellationToken, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步送出物件
        /// </summary>
        /// <typeparam name="T">類型</typeparam>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">物件內容將轉換為Json</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        public static async Task SendJsonAsync<T>(this WebSocket obj, T data, Encoding encoding, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            await obj.SendJsonAsync<T>(data, encoding, CancellationToken.None, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步送出物件
        /// </summary>
        /// <typeparam name="T">類型</typeparam>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">物件內容將轉換為Json</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        public static async Task SendJsonAsync<T>(this WebSocket obj, T data, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            await obj.SendJsonAsync<T>(data, Encoding.UTF8, bufferSize, millisecondsTimeout);
        }
        
        /// <summary>
        /// 非同步接收物件
        /// </summary>
        /// <typeparam name="T">類型</typeparam>
        /// <param name="obj">擴充對象</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的物件</returns>
        public static async Task<T> ReceiveJsonAsync<T>(this WebSocket obj, Encoding encoding, CancellationToken cancellationToken, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return JToken.Parse(encoding.GetString(await obj.ReceiveAsync(cancellationToken, bufferSize, millisecondsTimeout))).ToObject<T>();
        }

        /// <summary>
        /// 非同步接收物件(UTF8)
        /// </summary>
        /// <typeparam name="T">類型</typeparam>
        /// <param name="obj">擴充對象</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的物件</returns>
        public static async Task<T> ReceiveJsonAsync<T>(this WebSocket obj, CancellationToken cancellationToken, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return await obj.ReceiveJsonAsync<T>(Encoding.UTF8,cancellationToken,bufferSize,millisecondsTimeout);
        }

        /// <summary>
        /// 非同步接收物件
        /// </summary>
        /// <typeparam name="T">類型</typeparam>
        /// <param name="obj">擴充對象</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的物件</returns>
        public static async Task<T> ReceiveJsonAsync<T>(this WebSocket obj, Encoding encoding, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return await obj.ReceiveJsonAsync<T>(encoding, CancellationToken.None, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步接收Json資料(UTF8)
        /// </summary>
        /// <typeparam name="T">類型</typeparam>
        /// <param name="obj">擴充對象</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的Json</returns>
        public static async Task<T> ReceiveJsonAsync<T>(this WebSocket obj, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return await obj.ReceiveJsonAsync<T>(CancellationToken.None, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步送出物件並等候接收
        /// </summary>
        /// <typeparam name="T">類型</typeparam>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">內容</param>
        /// <param name="encoding">文字編碼</param>
        /// <param name="cancellationToken">散佈通知的語彙基元，該通知表示不應取消作業</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的物件</returns>
        public static async Task<T> SendAndReceiveJsonAsync<T>(this WebSocket obj, T data, Encoding encoding, CancellationToken cancellationToken, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            T result = default(T);
            var notTimeout = await TaskFactory.LimitedTask(async () => {
                await obj.SendJsonAsync<T>(data, encoding, cancellationToken, bufferSize);
                result = await obj.ReceiveJsonAsync<T>(cancellationToken, bufferSize);
            }, millisecondsTimeout);
            if (!notTimeout) throw new TimeoutException();
            return result;
        }

        /// <summary>
        /// 非同步送出物件並等候接收
        /// </summary>
        /// <typeparam name="T">類型</typeparam>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">內容</param>
        /// <param name="encoding">文字編碼(</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的物件</returns>
        public static async Task<T> SendAndReceiveJsonAsync<T>(this WebSocket obj, T data, Encoding encoding, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return await obj.SendAndReceiveJsonAsync(data, encoding, CancellationToken.None, bufferSize, millisecondsTimeout);
        }

        /// <summary>
        /// 非同步送出物件(UTF8)並等候接收
        /// </summary>
        /// <typeparam name="T">類型</typeparam>
        /// <param name="obj">擴充對象</param>
        /// <param name="data">內容</param>
        /// <param name="bufferSize">緩衝區大小</param>
        /// <param name="millisecondsTimeout">逾時限制</param>
        /// <returns>接收到的物件</returns>
        public static async Task<T> SendAndReceiveJsonAsync<T>(this WebSocket obj, T data, int bufferSize = 1024 * 4, int millisecondsTimeout = -1) {
            return await obj.SendAndReceiveJsonAsync(data, Encoding.UTF8, bufferSize, millisecondsTimeout);
        }
    }
}