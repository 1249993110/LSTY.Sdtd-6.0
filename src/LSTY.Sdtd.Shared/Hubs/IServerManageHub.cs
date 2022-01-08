using LSTY.Sdtd.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Hubs
{
    public interface IServerManageHub
    {
        Task<IEnumerable<string>> ExecuteConsoleCommand(string command, bool inMainThread = false);

        Task<LivePlayer> GetPlayer(int entityId);
        Task<IEnumerable<LivePlayer>> GetPlayers();
        Task<int> GetPlayerCount();
        Task<Inventory> GetPlayerInventory(int entityId);
        Task<IEnumerable<PlayerInventory>> GetPlayerInventories();

        Task<byte[]> GetItemIcon(string iconName);

        Task<IEnumerable<string>> AddAdmins(IEnumerable<AdminEntry> admins);
        Task<IEnumerable<string>> RemoveAdmins(IEnumerable<string> userIdentifiers);
        Task<IEnumerable<AdminEntry>> GetAdmins();

        Task<IEnumerable<string>> AddPermissions(IEnumerable<PermissionEntry> permissions);
        Task<IEnumerable<string>> RemovePermissions(IEnumerable<string> command);
        Task<IEnumerable<PermissionEntry>> GetPermissions();

        Task<IEnumerable<string>> AddWhitelist(IEnumerable<WhitelistEntry> whitelist);
        Task<IEnumerable<string>> RemoveWhitelist(IEnumerable<string> userIdentifiers);
        Task<IEnumerable<WhitelistEntry>> GetWhitelist();

        Task<IEnumerable<string>> AddBlacklist(IEnumerable<BlacklistEntry> blacklist);
        Task<IEnumerable<string>> RemoveBlacklist(IEnumerable<string> userIdentifiers);
        Task<IEnumerable<BlacklistEntry>> GetBlacklist();

        /// <summary>
        /// Sends a global message to all connected clients.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> SendGlobalMessage(GlobalMessage globalMessage);
        /// <summary>
        /// Sends a private message to single connected client.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> SendPrivateMessage(PrivateMessage privateMessage);

        Task<IEnumerable<string>> TeleportPlayer(TeleportEntry teleportEntry);
        Task<IEnumerable<string>> GiveItem(GiveItemEntry giveItemEntry);
        Task<IEnumerable<string>> SpawnEntity(SpawnEntityEntry spawnEntityEntry);
        Task<IEnumerable<AllowSpawnedEntity>> GetAllowSpawnedEntities();
        Task<IEnumerable<AllowedCommandEntry>> GetAllowedCommands();
        Task<Stats> GetStats();

        Task<IEnumerable<string>> RestartServer(bool force = false);

        Task<IEnumerable<EntityLocation>> GetAnimalsLocation();
        Task<IEnumerable<EntityLocation>> GetHostileLocation();
        Task<IEnumerable<PlayerLocation>> GetPlayersLocation();

        Task<LandClaims> GetLandClaims();
        Task<ClaimOwner> GetLandClaim(int entityId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSzie">每页数量，值为 -1 时返回所有记录</param>
        /// <returns></returns>
        Task<ItemBlockPaged> GetItemBlocks(int pageIndex = 1, int pageSzie = 10);


        Task<IDictionary<string, string>> GetLocalization([DefaultValue("schinese")] string language);
        Task<string> GetLocalization(string itemName, [DefaultValue("schinese")] string language);

        Task<IEnumerable<string>> GetKnownLanguages();
    }
}
