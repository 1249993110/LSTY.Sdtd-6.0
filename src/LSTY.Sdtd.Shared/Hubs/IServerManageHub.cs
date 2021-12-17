using LSTY.Sdtd.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Hubs
{
    public interface IServerManageHub
    {
        Task<List<string>> ExecuteConsoleCommand(string command, bool inMainThread = false);

        Task<OnlinePlayer> GetOnlinePlayer(int entityId);

        Task<List<OnlinePlayer>> GetOnlinePlayers();

        Task<int> GetOnlinePlayerCount();
    }
}
