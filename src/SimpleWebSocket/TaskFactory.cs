using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleWebSocket {
    public static class TaskFactory {
        /// <summary>
        /// 等待<see cref="Action"/>物件在指定的毫秒限制內完成執行，否則強制結束。
        /// </summary>
        /// <param name="action">要執行的動作委派</param>
        /// <param name="millisecondsTimeout">要等候的毫秒數，如果要無限期等候，則為<see cref="System.Threading.Timeout.Infinite"/>(-1)。</param>
        /// <returns>委派是否在指定的毫秒內完成執行</returns>
        public static async Task<bool> LimitedTask(Action action, int millisecondsTimeout) {
            if(millisecondsTimeout == -1){
                action.Invoke();
                return true;
            }
            return await Task.Run(() => {
                var tokenSource = new CancellationTokenSource();

                Task task = Task.Factory.StartNew(action, tokenSource.Token);
                bool result = false;
                if (!(result = task.Wait(millisecondsTimeout))) {
                    tokenSource.Cancel(false);
                }
                return result;
            });
        }
    }
}
