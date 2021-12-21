using IceCoffee.AspNetCore.Authentication;
using LSTY.Sdtd.Services.Managers;
using LSTY.Sdtd.WebApi.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace LSTY.Sdtd.WebApi.Middlewares
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<WebSocketMiddleware> _logger;
        private readonly AppSettings _appSettings;
        private readonly ConcurrentDictionary<string, WebSocket> _clients;


        private struct RecvLoopEntry
        {
            public string ConnectionId;
            public WebSocket WebSocket;
            public TaskCompletionSource SocketFinishedTcs;
        }

        private struct SendLoopEntry
        {
            public string ConnectionId;
            public WebSocket WebSocket;
            public string Message;
        }

        public WebSocketMiddleware(RequestDelegate next, ILogger<WebSocketMiddleware> logger, IOptions<AppSettings> options, SignalRManager signalRManager)
        {
            _next = next;
            _logger = logger;
            _appSettings = options.Value;
            _clients = new ConcurrentDictionary<string, WebSocket>();
            signalRManager.ModEventHookHub.LogCallback += On_ModEventHookHub_LogCallback;
        }

        private void On_ModEventHookHub_LogCallback(string message)
        {
            foreach (var client in _clients)
            {
                string connectionId = client.Key;
                var webSocket = client.Value;
                try
                {
                    if (webSocket.State == WebSocketState.Open)
                    {
                        Task.Factory.StartNew(SendMessage, new SendLoopEntry() 
                        { 
                            ConnectionId = connectionId, 
                            WebSocket = webSocket, 
                            Message = message 
                        });
                    }
                    else
                    {
                        _clients.TryRemove(client.Key, out var abnormalWebSocket);
                        abnormalWebSocket.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in WebSocketMiddleware.On_ModEventHookHub_LogCallback, Connection Id: " + connectionId);
                }
            }
        }

        private async Task SendMessage(object state)
        {
            var sendLoopEntry = (SendLoopEntry)state;
            try
            {
                var data = Encoding.UTF8.GetBytes(sendLoopEntry.Message);
                var buffer = new ArraySegment<byte>(data, 0, data.Length);
                await sendLoopEntry.WebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in WebSocketMiddleware.SendMessage, Connection Id: " + sendLoopEntry.ConnectionId);
            }
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        if(string.IsNullOrEmpty(_appSettings.AccessToken) == false)
                        {
                            // Unauthorized
                            if (context.Request.Query.TryGetValue(ApiKeyAuthenticationHandler.HttpRequestHeaderName, out var value) == false
                                || value != _appSettings.AccessToken)
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                return;
                            }
                        }

                        using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                        {
                            string connectionId = context.Connection.Id;
                            var socketFinishedTcs = new TaskCompletionSource();

                            _logger.LogInformation("WebSocket connection started, connection Id: " + connectionId);

                            await Task.Factory.StartNew(RecvLoop, new RecvLoopEntry()
                            {
                                ConnectionId = connectionId,
                                WebSocket = webSocket,
                                SocketFinishedTcs = socketFinishedTcs
                            });

                            _clients.TryAdd(connectionId, webSocket);

                            await socketFinishedTcs.Task;

                            _logger.LogInformation("WebSocket connection closed, connection Id: " + connectionId);
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    await _next(context);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in WebSocketMiddleware.Invoke");
            }
        }

        private async Task RecvLoop(object state)
        {
            var recvLoopEntry = (RecvLoopEntry)state;
            string connectionId = recvLoopEntry.ConnectionId;
            var webSocket = recvLoopEntry.WebSocket;
            var socketFinishedTcs = recvLoopEntry.SocketFinishedTcs;

            try
            {
                var buffer = new byte[1024];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                while (result.CloseStatus.HasValue == false)
                {
                    result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                }

                _clients.TryRemove(connectionId, out var _);

                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

                socketFinishedTcs.SetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in WebSocketMiddleware.RecvLoop, Connection Id: " + connectionId);
            }
        }
    }
}
