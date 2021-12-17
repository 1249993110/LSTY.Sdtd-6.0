using LSTY.Sdtd.Services.HubReceivers;
using LSTY.Sdtd.Shared.Models;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Services
{
    public class OnlinePlayers : ConcurrentDictionary<string, OnlinePlayer>, IOnlinePlayers
    {
        private readonly ServerManageHubReceiver _serverManageHubReceiver;
        private readonly SignalRManager _signalRManager;

        public event Action ServerNonePlayer;
        public event Action ServerHavePlayerAgain;

        public OnlinePlayers(ModEventHookHubReceiver modEventHookHubReceiver,
            ServerManageHubReceiver serverManageHubReceiver,
            SignalRManager signalRManager)
        {
            modEventHookHubReceiver.PlayerDisconnected += OnPlayerDisconnected;
            modEventHookHubReceiver.SavePlayerData += OnSavePlayerData;
            modEventHookHubReceiver.PlayerSpawning += OnPlayerSpawning;
            this._serverManageHubReceiver = serverManageHubReceiver;
            this._signalRManager = signalRManager;
            _signalRManager.Connected += OnSignalRManager_Connected;
            _signalRManager.Disconnected += OnSignalRManager_Disconnected;
        }

        private void OnSignalRManager_Connected()
        {
            var onlinePlayers = _serverManageHubReceiver.GetOnlinePlayers().Result;
            foreach (var onlinePlayer in onlinePlayers)
            {
                base[onlinePlayer.SteamId] = onlinePlayer;
            }

            if(onlinePlayers.Count > 0)
            {
                ServerHavePlayerAgain?.Invoke();
            }
        }

        private void OnSignalRManager_Disconnected()
        {
            base.Clear();
            ServerNonePlayer?.Invoke();
        }

        IEnumerable<string> IOnlinePlayers.Keys => Keys;

        IEnumerable<OnlinePlayer> IOnlinePlayers.Values => Values;

        OnlinePlayer IOnlinePlayers.this[string steamId] 
        {
            get 
            {
                if(TryGetPlayer(steamId, out var onlinePlayer))
                {
                    return onlinePlayer;
                }

                throw new KeyNotFoundException("SteamId not found in online players");
            }
        }

        public bool TryGetPlayer(string steamId, out OnlinePlayer onlinePlayer)
        {
            if (base.TryGetValue(steamId, out onlinePlayer) == false)
            {
                onlinePlayer = _serverManageHubReceiver.GetOnlinePlayer(steamId).Result;
                if(onlinePlayer == null)
                {
                    return false;
                }
                else
                {
                    // 插入或更新项
                    base[steamId] = onlinePlayer;
                }
            }

            return true;
        }

        private void OnPlayerDisconnected(string steamId)
        {
            base.TryRemove(steamId, out _);

            if (base.IsEmpty && _serverManageHubReceiver.GetOnlinePlayerCount().Result == 0)
            {
                ServerNonePlayer?.Invoke();
            }
        }

        private void OnSavePlayerData(OnlinePlayer onlinePlayer)
        {
            base[onlinePlayer.SteamId] = onlinePlayer;
        }

        private void OnPlayerSpawning(OnlinePlayer onlinePlayer)
        {
            bool isEmpty = base.IsEmpty;

            base[onlinePlayer.SteamId] = onlinePlayer;

            if (isEmpty && _serverManageHubReceiver.GetOnlinePlayerCount().Result == 1)
            {
                ServerHavePlayerAgain?.Invoke();
            }
        }
    }
}
