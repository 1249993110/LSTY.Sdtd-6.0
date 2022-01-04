using IceCoffee.AspNetCore.Authentication;
using LSTY.Sdtd.Services;
using LSTY.Sdtd.Services.Managers;
using LSTY.Sdtd.Shared.Models;
using LSTY.Sdtd.WebApi.Models;
using MessagePack;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace LSTY.Sdtd.WebApi.Middlewares
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<WebSocketMiddleware> _logger;
        private readonly AppSettings _appSettings;
        private readonly ConcurrentDictionary<string, WebSocket> _clients;
        private readonly ILivePlayers _livePlayers;


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
            public WebSocketMessage WebSocketMessage;
        }

        public WebSocketMiddleware(RequestDelegate next, ILogger<WebSocketMiddleware> logger, IOptions<AppSettings> options,
            SignalRManager signalRManager, ILivePlayers livePlayers)
        {
            _next = next;
            _logger = logger;
            _appSettings = options.Value;
            _clients = new ConcurrentDictionary<string, WebSocket>();
            signalRManager.ModEventHookHub.LogCallback += On_ModEventHookHub_LogCallback;
            signalRManager.ModEventHookHub.ChatMessage += On_ModEventHookHub_ChatMessage;
            signalRManager.ModEventHookHub.SavePlayerData += On_ModEventHookHub_SavePlayerData;
            _livePlayers = livePlayers;
        }



        private void On_ModEventHookHub_LogCallback(LogEntry logEntry)
        {
            SendMessageToAllClients(new WebSocketMessage()
            {
                MessageType = Shared.Models.WebSocketMessageType.ConsoleLog,
                MessageEntity = logEntry
            });
        }
        private void On_ModEventHookHub_ChatMessage(ChatMessage chatMessage)
        {
            SendMessageToAllClients(new WebSocketMessage() 
            {
                MessageType = Shared.Models.WebSocketMessageType.ChatMessage,
                MessageEntity = chatMessage
            });
        }

        private void On_ModEventHookHub_SavePlayerData(LivePlayer livePlayer)
        {
            SendMessageToAllClients(new WebSocketMessage()
            {
                MessageType = Shared.Models.WebSocketMessageType.PlayerUpdate,
                MessageEntity = _livePlayers.Values
            });
        }

        private void SendMessageToAllClients(WebSocketMessage webSocketMessage)
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
                            WebSocketMessage = webSocketMessage
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
                    _logger.LogError(ex, "Error in WebSocketMiddleware.SendMessage, Connection Id: " + connectionId);
                }
            }
        }

        private async Task SendMessage(object state)
        {
            var sendLoopEntry = (SendLoopEntry)state;
            try
            {
                var serializedContent = JsonSerializer.Serialize(sendLoopEntry.WebSocketMessage, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var data = Encoding.UTF8.GetBytes(serializedContent);

                var buffer = new ArraySegment<byte>(data, 0, data.Length);
                await sendLoopEntry.WebSocket.SendAsync(buffer, System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in WebSocketMiddleware.SendMessage, Connection Id: " + sendLoopEntry.ConnectionId);
            }
        }

        public async Task Invoke(HttpContext context)
        {

            if (context.Request.Path == "/ws")
            {
                try
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        if (string.IsNullOrEmpty(_appSettings.AccessToken) == false)
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in WebSocketMiddleware.Invoke");
                }
            }
            else
            {
                await _next(context);
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

                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

                socketFinishedTcs.SetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in WebSocketMiddleware.RecvLoop, Connection Id: " + connectionId);
            }
            finally
            {
                _clients.TryRemove(connectionId, out var _);
            }
        }
    }
}
