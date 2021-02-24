using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.WebSockets;
using System.Threading;
using WebSocketServer.Middleware;

namespace WebSocketServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        { 
        }   

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            //app.UseHttpsRedirection();

            app.UseWebSockets();
            app.UseWebSocketServer();

            app.Run(async context => 
            {
                Console.WriteLine("Hello from the 3rd request delegate");
                await context.Response.WriteAsync("Hello from the 3rd request delegate");
            });
        }



        public void WriteRequestParam(HttpContext context)
        {
            Console.WriteLine("Request Method: "+ context.Request.Method);
            Console.WriteLine("Request Method: "+ context.Request.Protocol);
        
        if(context.Request.Headers != null){
            foreach(var h in context.Request.Headers){
                Console.WriteLine("---> "+h.Key +" : " + h.Value);
            }
        }
        }
    } 
}
