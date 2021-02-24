using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace WebSocketServer.Middleware
{
    public static class WebSocketServerMiddlewareExtensions
    {
        public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder builder){
            return builder.UseMiddleware<WebSocketServerMiddleware>();
        }

         public static IServiceCollection AddWebSocketManager(this IServiceCollection services){
             services.AddSingleton<WebSocketServerConnectionManager>();
             return services;
        }

    }
}