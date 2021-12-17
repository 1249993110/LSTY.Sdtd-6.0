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

        public async Task<OnlinePlayer> GetOnlinePlayer(int entityId)
        {
            return await Task.Factory.StartNew((state) =>
            {
                return ConnectionManager.Instance.Clients.ForEntityId((int)state)?.ToOnlinePlayer();
            }, entityId);
        }

        public async Task<List<OnlinePlayer>> GetOnlinePlayers()
        {
            return await Task.Run(() =>
            {
                List<OnlinePlayer> list = new List<OnlinePlayer>();
                foreach (var client in ConnectionManager.Instance.Clients.List)
                {
                    list.Add(client.ToOnlinePlayer());
                }

                return list;
            });
        }

        public async Task<int> GetOnlinePlayerCount()
        {
            return await Task.FromResult(GameManager.Instance.World.Players.Count);
        }
    }
}
