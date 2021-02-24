using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Linq;



namespace WebSocketServer.Middleware
{
    public class WebSocketServerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketServerConnectionManager _manager;
        
        public WebSocketServerMiddleware(RequestDelegate next,WebSocketServerConnectionManager manager)
        {
            _next = next;
            _manager = manager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    Console.WriteLine("WebSocket connected");

                    var ConnID = _manager.AddSocket(webSocket);
                    await SendConnIDAsync(webSocket,ConnID);

                    await ReceiveMessage(webSocket,async (result,buffer) => 
                    {
                        if(result.MessageType == WebSocketMessageType.Text)
                        {
                            Console.WriteLine("Message Receive");
                            Console.WriteLine($"Message: {Encoding.UTF8.GetString(buffer,0,result.Count)}");
                        
                            await RouteJSONMessageAsync(Encoding.UTF8.GetString(buffer,0,result.Count));
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

        private async Task SendConnIDAsync(WebSocket socket,string connID)
        {
            var buffer = Encoding.UTF8.GetBytes("ConnID: "+connID);
            await socket.SendAsync(buffer,WebSocketMessageType.Text,true,CancellationToken.None);
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

        public async Task RouteJSONMessageAsync(string message)
        {
            var routeOb = JsonConvert.DeserializeObject<dynamic>(message);

            if(Guid.TryParse(routeOb.To.ToString(),out Guid guidOut))
            {
                Console.WriteLine("Targeted");
                var sock = _manager.GetAllSockets().FirstOrDefault(s => s.Key ==  guidOut.ToString());
                if(sock.Value != null)
                {
                    if(sock.Value.State == WebSocketState.Open)
                    {
                        await sock.Value.SendAsync(Encoding.UTF8.GetBytes(routeOb.Message.ToString()), WebSocketMessageType.Text,true,CancellationToken.None);
                    }
                }
                else
                {
                    Console.WriteLine("Invalid person");
                }
            }
            else
            {
                Console.WriteLine("BROADCAST");
                foreach(var sockOpen in _manager.GetAllSockets())
                {
                    if(sockOpen.Value.State == WebSocketState.Open)
                    {
                        await sockOpen.Value.SendAsync(Encoding.UTF8.GetBytes(routeOb.Message.ToString()), WebSocketMessageType.Text,true,CancellationToken.None);
                    }
                }
            }
        }
    }
}