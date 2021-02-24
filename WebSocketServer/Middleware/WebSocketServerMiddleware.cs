using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;



namespace WebSocketServer.Middleware
{
    public class WebSocketServerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketServerConnectionManager _manager = new WebSocketServerConnectionManager();
        
        public WebSocketServerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    Console.WriteLine("WebSocket connected");

                    var ConnID = _manager.AddSocket(webSocket);

                    await ReceiveMessage(webSocket,async (result,buffer) => 
                    {
                        if(result.MessageType == WebSocketMessageType.Text)
                        {
                            Console.WriteLine("Message Receive");
                            Console.WriteLine($"Message: {Encoding.UTF8.GetString(buffer,0,result.Count)}");
                            return;
                        }
                        else if(result.MessageType == WebSocketMessageType.Close)
                        {
                            Console.WriteLine("Close message");
                            return;
                        }

                    });
                }
                else
                {
                    Console.WriteLine("Hello from the 2nd request delegate");
                    await _next(context);
                }
        }


        
        private async Task ReceiveMessage(WebSocket socket,Action<WebSocketReceiveResult,byte[]> handleMessage)
        {
            var buffer = new byte[1024*4];
            while(socket.State == WebSocketState.Open)
            {
                var resultat = await socket.ReceiveAsync(buffer,CancellationToken.None);
                handleMessage(resultat,buffer);
            }

        }
    }
}