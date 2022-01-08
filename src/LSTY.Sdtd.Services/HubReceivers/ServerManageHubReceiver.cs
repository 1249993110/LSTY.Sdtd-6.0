using LSTY.Sdtd.Shared.Hubs;
using LSTY.Sdtd.Shared.Models;
using Microsoft.AspNet.SignalR.Client;
using System.ComponentModel;

namespace LSTY.Sdtd.Services.HubReceivers
{
    public class ServerManageHubReceiver : IServerManageHub
    {
        private readonly IHubProxy _hubProxy;

        public ServerManageHubReceiver(HubConnection hubConnection)
        {
            _hubProxy = hubConnection.CreateHubProxy(nameof(IServerManageHub));
        }

        public Task<IEnumerable<string>> ExecuteConsoleCommand(string command, bool inMainThread = false)
        {
            return _hubProxy.Invoke<IEnumerable<string>>(nameof(IServerManageHub.ExecuteConsoleCommand), new object[] { command, inMainThread });
        }

        public Task<LivePlayer> GetPlayer(int entityId)
        {
            return _hubProxy.Invoke<LivePlayer>(nameof(IServerManageHub.GetPlayer), new object[] { entityId });
        }

        public Task<IEnumerable<LivePlayer>> GetPlayers()
        {
            return _hubProxy.Invoke<IEnumerable<LivePlayer>>(nameof(IServerManageHub.GetPlayers));
        }

        public Task<int> GetPlayerCount()
        {
            return _hubProxy.Invoke<int>(nameof(IServerManageHub.GetPlayerCount));
        }

        public Task<Inventory> GetPlayerInventory(int entityId)
        {
            return _hubProxy.Invoke<Inventory>(nameof(IServerManageHub.GetPlayerInventory), new object[] { entityId });
        }

        public Task<IEnumerable<PlayerInventory>> GetPlayerInventories()
        {
            return _hubProxy.Invoke<IEnumerable<PlayerInventory>>(nameof(IServerManageHub.GetPlayerInventories));
        }

        public Task<byte[]> GetItemIcon(string iconName)
        {
            return _hubProxy.Invoke<byte[]>(nameof(IServerManageHub.GetItemIcon), new object[] { iconName });
        }

        public Task<IEnumerable<string>> AddAdmins(IEnumerable<AdminEntry> admins)
        {
            return _hubProxy.Invoke<IEnumerable<string>>(nameof(IServerManageHub.AddAdmins), new object[] { admins });
        }

        public Task<IEnumerable<string>> RemoveAdmins(IEnumerable<string> userIdentifiers)
        {
            return _hubProxy.Invoke<IEnumerable<string>>(nameof(IServerManageHub.RemoveAdmins), new object[] { userIdentifiers });
        }

        public Task<IEnumerable<AdminEntry>> GetAdmins()
        {
            return _hubProxy.Invoke<IEnumerable<AdminEntry>>(nameof(IServerManageHub.GetAdmins));
        }

        public Task<IEnumerable<string>> AddPermissions(IEnumerable<PermissionEntry> permissions)
        {
            return _hubProxy.Invoke<IEnumerable<string>>(nameof(IServerManageHub.AddPermissions), new object[] { permissions });
        }

        public Task<IEnumerable<string>> RemovePermissions(IEnumerable<string> command)
        {
            return _hubProxy.Invoke<IEnumerable<string>>(nameof(IServerManageHub.RemovePermissions), new object[] { command });
        }

        public Task<IEnumerable<PermissionEntry>> GetPermissions()
        {
            return _hubProxy.Invoke<IEnumerable<PermissionEntry>>(nameof(IServerManageHub.GetPermissions));
        }

        public Task<IEnumerable<string>> AddWhitelist(IEnumerable<WhitelistEntry> whitelist)
        {
            return _hubProxy.Invoke<IEnumerable<string>>(nameof(IServerManageHub.AddWhitelist), new object[] { whitelist });
        }

        public Task<IEnumerable<string>> RemoveWhitelist(IEnumerable<string> userIdentifiers)
        {
            return _hubProxy.Invoke<IEnumerable<string>>(nameof(IServerManageHub.RemoveWhitelist), new object[] { userIdentifiers });
        }

        public Task<IEnumerable<WhitelistEntry>> GetWhitelist()
        {
            return _hubProxy.Invoke<IEnumerable<WhitelistEntry>>(nameof(IServerManageHub.GetWhitelist));
        }

        public Task<IEnumerable<string>> AddBlacklist(IEnumerable<BlacklistEntry> blacklist)
        {
            return _hubProxy.Invoke<IEnumerable<string>>(nameof(IServerManageHub.AddBlacklist), new object[] { blacklist });
        }

        public Task<IEnumerable<string>> RemoveBlacklist(IEnumerable<string> userIdentifiers)
        {
            return _hubProxy.Invoke<IEnumerable<string>>(nameof(IServerManageHub.RemoveBlacklist), new object[] { userIdentifiers });
        }

        public Task<IEnumerable<BlacklistEntry>> GetBlacklist()
        {
            return _hubProxy.Invoke<IEnumerable<BlacklistEntry>>(nameof(IServerManageHub.GetBlacklist));
        }

        public Task<IEnumerable<string>> SendGlobalMessage(GlobalMessage globalMessage)
        {
            return _hubProxy.Invoke<IEnumerable<string>>(nameof(IServerManageHub.SendGlobalMessage), new object[] { globalMessage });
        }

        public Task<IEnumerable<string>> SendPrivateMessage(PrivateMessage privateMessage)
        {
            return _hubProxy.Invoke<IEnumerable<string>>(nameof(IServerManageHub.SendPrivateMessage), new object[] { privateMessage });
        }

        public Task<IEnumerable<string>> TeleportPlayer(TeleportEntry teleportEntry)
        {
            return _hubProxy.Invoke<IEnumerable<string>>(nameof(IServerManageHub.TeleportPlayer), new object[] { teleportEntry });
        }

        public Task<IEnumerable<string>> GiveItem(GiveItemEntry giveItemEntry)
        {
            return _hubProxy.Invoke<IEnumerable<string>>(nameof(IServerManageHub.GiveItem), new object[] { giveItemEntry });
        }

        public Task<IEnumerable<string>> SpawnEntity(SpawnEntityEntry spawnEntityEntry)
        {
            return _hubProxy.Invoke<IEnumerable<string>>(nameof(IServerManageHub.SpawnEntity), new object[] { spawnEntityEntry });
        }

        public Task<IEnumerable<AllowSpawnedEntity>> GetAllowSpawnedEntities()
        {
            return _hubProxy.Invoke<IEnumerable<AllowSpawnedEntity>>(nameof(IServerManageHub.GetAllowSpawnedEntities));
        }

        public Task<IEnumerable<AllowedCommandEntry>> GetAllowedCommands()
        {
            return _hubProxy.Invoke<IEnumerable<AllowedCommandEntry>>(nameof(IServerManageHub.GetAllowedCommands));
        }

        public Task<Stats> GetStats()
        {
            return _hubProxy.Invoke<Stats>(nameof(IServerManageHub.GetStats));
        }

        public Task<IEnumerable<string>> RestartServer(bool force = false)
        {
            return _hubProxy.Invoke<IEnumerable<string>>(nameof(IServerManageHub.RestartServer), new object[] { force });
        }


        public Task<IEnumerable<EntityLocation>> GetAnimalsLocation()
        {
            return _hubProxy.Invoke<IEnumerable<EntityLocation>>(nameof(IServerManageHub.GetAnimalsLocation));
        }

        public Task<IEnumerable<EntityLocation>> GetHostileLocation()
        {
            return _hubProxy.Invoke<IEnumerable<EntityLocation>>(nameof(IServerManageHub.GetHostileLocation));
        }

        public Task<IEnumerable<PlayerLocation>> GetPlayersLocation()
        {
            return _hubProxy.Invoke<IEnumerable<PlayerLocation>>(nameof(IServerManageHub.GetPlayersLocation));
        }

        public Task<LandClaims> GetLandClaims()
        {
            return _hubProxy.Invoke<LandClaims>(nameof(IServerManageHub.GetLandClaims));
        }

        public Task<ClaimOwner> GetLandClaim(int entityId)
        {
            return _hubProxy.Invoke<ClaimOwner>(nameof(IServerManageHub.GetLandClaim), new object[] { entityId });
        }

        public Task<ItemBlockPaged> GetItemBlocks(int pageIndex = 1, int pageSzie = 10)
        {
            return _hubProxy.Invoke<ItemBlockPaged>(nameof(IServerManageHub.GetItemBlocks), new object[] { pageIndex, pageSzie });
        }

        public Task<IDictionary<string, string>> GetLocalization([DefaultValue("schinese")] string language)
        {
            return _hubProxy.Invoke<IDictionary<string, string>>(nameof(IServerManageHub.GetLocalization), new object[] { language });
        }

        public Task<string> GetLocalization(string itemName, [DefaultValue("schinese")] string language)
        {
            return _hubProxy.Invoke<string>(nameof(IServerManageHub.GetLocalization), new object[] { itemName, language });
        }

        public Task<IEnumerable<string>> GetKnownLanguages()
        {
            return _hubProxy.Invoke<IEnumerable<string>>(nameof(IServerManageHub.GetKnownLanguages));
        }
    }
}