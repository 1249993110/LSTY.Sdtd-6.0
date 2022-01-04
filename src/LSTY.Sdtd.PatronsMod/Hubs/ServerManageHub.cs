using LSTY.Sdtd.PatronsMod.Extensions;
using LSTY.Sdtd.Shared;
using LSTY.Sdtd.Shared.Hubs;
using LSTY.Sdtd.Shared.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Platform.Local;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.PatronsMod
{
    [HubName(nameof(IServerManageHub))]
    public class ServerManageHub : Hub, IServerManageHub
    {
        public async Task<List<string>> ExecuteConsoleCommand(string command, bool inMainThread = false)
        {
            if (inMainThread == false)
            {
                return await Task.Factory.StartNew((state) =>
                {
                    return SdtdConsole.Instance.ExecuteSync((string)state, ModApi.GetCmdExecuteDelegate());
                }, command);
            }
            else
            {
                return await Task.Factory.StartNew((state) =>
                {
                    List<string> executeResult = null;
                    ModApi.MainThreadContext.Send((state1) =>
                    {
                        executeResult = SdtdConsole.Instance.ExecuteSync((string)state1, ModApi.GetCmdExecuteDelegate());
                    }, state);

                    return executeResult;
                }, command);
            }
            
        }

        public async Task<LivePlayer> GetLivePlayer(int entityId)
        {
            return await Task.Factory.StartNew((state) =>
            {
                return ConnectionManager.Instance.Clients.ForEntityId((int)state)?.ToLivePlayer();
            }, entityId);
        }

        public async Task<List<LivePlayer>> GetLivePlayers()
        {
            return await Task.Run(() =>
            {
                var list = new List<LivePlayer>();
                foreach (var client in ConnectionManager.Instance.Clients.List)
                {
                    var livePlayer = client.ToLivePlayer();
                    if (livePlayer != null)
                    {
                        list.Add(livePlayer);
                    }
                }

                return list;
            });
        }

        public async Task<int> GetLivePlayerCount()
        {
            return await Task.FromResult(GameManager.Instance.World.Players.Count);
        }

        public async Task<Shared.Models.Inventory> GetLivePlayerInventory(int entityId)
        {
            return await Task.Factory.StartNew((state) =>
            {
                return ConnectionManager.Instance.Clients.ForEntityId((int)state)?.latestPlayerData.GetInventory();
            }, entityId);
        }

        public async Task<byte[]> GetItemIcon(string iconName)
        {
            return await Task.Factory.StartNew((state) =>
            {
                string iconPath = FindIconPath((string)state);
                if (iconPath == null)
                {
                    return null;
                }
                else
                {
                    return File.ReadAllBytes(iconPath);
                }
            }, iconName);
        }

        private static string FindIconPath(string iconName)
        {
            string path = "Data/ItemIcons/" + iconName;
            if (File.Exists(path))
            {
                return path;
            }

            foreach (Mod mod in ModManager.GetLoadedMods())
            {
                path = Path.Combine(mod.Path, "ItemIcons", iconName);
                if (File.Exists(path))
                {
                    return path;
                }

                foreach (string dir in Directory.GetDirectories(mod.Path))
                {
                    path = Path.Combine(dir, iconName);
                    if (File.Exists(path))
                    {
                        return path;
                    }

                    foreach (string subDir in Directory.GetDirectories(dir))
                    {
                        path = Path.Combine(subDir, iconName);
                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }
                }
            }

            return null;
        }
    }
}
