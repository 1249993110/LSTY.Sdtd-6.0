using LSTY.Sdtd.Shared.Hubs;
using LSTY.Sdtd.Shared.Models;
using Microsoft.AspNet.SignalR.Client;

namespace LSTY.Sdtd.Services.HubReceivers
{
    public class ServerManageHubReceiver : IServerManageHub
    {
        private readonly SignalRManager _signalRManager;
        private IHubProxy _hubProxy;

        public ServerManageHubReceiver(SignalRManager signalRManager)
        {
            _signalRManager = signalRManager;
            _signalRManager.Ready += OnSignalRManager_Ready;
        }

        private void OnSignalRManager_Ready(HubConnection hubConnection)
        {
            _hubProxy = hubConnection.CreateHubProxy(nameof(IServerManageHub));
        }

        public Task<List<string>> ExecuteConsoleCommand(string command, bool inMainThread = false)
        {
            return _hubProxy.Invoke<List<string>>(nameof(IServerManageHub.ExecuteConsoleCommand), new object[] { command, inMainThread });
        }

        public Task<OnlinePlayer> GetOnlinePlayer(int entityId)
        {
            return _hubProxy.Invoke<OnlinePlayer>(nameof(IServerManageHub.GetOnlinePlayer), new object[] { entityId });
        }

        public Task<List<OnlinePlayer>> GetOnlinePlayers()
        {
            return _hubProxy.Invoke<List<OnlinePlayer>>(nameof(IServerManageHub.GetOnlinePlayers));
        }

        public Task<int> GetOnlinePlayerCount()
        {
            return _hubProxy.Invoke<int>(nameof(IServerManageHub.GetOnlinePlayerCount));
        }
    }
}