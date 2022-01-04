using LSTY.Sdtd.Shared.Hubs;
using LSTY.Sdtd.Shared.Models;
using Microsoft.AspNet.SignalR.Client;

namespace LSTY.Sdtd.Services.HubReceivers
{
    public class ServerManageHubReceiver : IServerManageHub
    {
        private readonly IHubProxy _hubProxy;

        public ServerManageHubReceiver(HubConnection hubConnection)
        {
            _hubProxy = hubConnection.CreateHubProxy(nameof(IServerManageHub));
        }

        public Task<List<string>> ExecuteConsoleCommand(string command, bool inMainThread = false)
        {
            return _hubProxy.Invoke<List<string>>(nameof(IServerManageHub.ExecuteConsoleCommand), new object[] { command, inMainThread });
        }

        public Task<LivePlayer> GetLivePlayer(int entityId)
        {
            return _hubProxy.Invoke<LivePlayer>(nameof(IServerManageHub.GetLivePlayer), new object[] { entityId });
        }

        public Task<List<LivePlayer>> GetLivePlayers()
        {
            return _hubProxy.Invoke<List<LivePlayer>>(nameof(IServerManageHub.GetLivePlayers));
        }

        public Task<int> GetLivePlayerCount()
        {
            return _hubProxy.Invoke<int>(nameof(IServerManageHub.GetLivePlayerCount));
        }

        public Task<Inventory> GetLivePlayerInventory(int entityId)
        {
            return _hubProxy.Invoke<Inventory>(nameof(IServerManageHub.GetLivePlayerInventory), new object[] { entityId });
        }

        public Task<byte[]> GetItemIcon(string iconName)
        {
            return _hubProxy.Invoke<byte[]>(nameof(IServerManageHub.GetItemIcon), new object[] { iconName });
        }
    }
}