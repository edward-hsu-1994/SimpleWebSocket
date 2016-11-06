using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleWebSocket {
    /// <summary>
    /// 針對<see cref="IApplicationBuilder"/>類型的擴充方法
    /// </summary>
    public static class IApplicationBuilderExtension {
        /// <summary>
        /// 將WebSocket服務加入服務空間，使用指定的WebSocket處理容器
        /// </summary>
        /// <typeparam name="Handler">處理容器型別，必須繼承自WebSocketHandler</typeparam>
        /// <param name="builder">擴充對象</param>
        /// <param name="options">WebSocket選項</param>
        public static IApplicationBuilder UseWebSockets<Handler>(this IApplicationBuilder builder, WebSocketOptions options = null) where Handler : WebSocketHandler, new() {
            if (options == null)
                builder.UseWebSockets();
            else
                builder.UseWebSockets(options);

            Handler handler = (Handler)Activator.CreateInstance(typeof(Handler));

            builder.Map(handler.RequestPath, WebSocketApi => {
                builder.Use(handler.Start);
            });

            return builder;
        }
    }
}
