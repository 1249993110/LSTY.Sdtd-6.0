using LSTY.Sdtd.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Hubs
{
    public interface IServerManageHub
    {
        Task<List<string>> ExecuteConsoleCommand(string command, bool inMainThread = false);

        Task<LivePlayer> GetLivePlayer(int entityId);

        Task<List<LivePlayer>> GetLivePlayers();

        Task<int> GetLivePlayerCount();

        Task<Inventory> GetLivePlayerInventory(int entityId);

        Task<byte[]> GetItemIcon(string iconName);
    }
}
