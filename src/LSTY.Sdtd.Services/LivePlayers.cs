using LSTY.Sdtd.Services.HubReceivers;
using LSTY.Sdtd.Services.Managers;
using LSTY.Sdtd.Shared.Hubs;
using LSTY.Sdtd.Shared.Models;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Services
{
    public class LivePlayers : ConcurrentDictionary<int, LivePlayer>, ILivePlayers
    {
        private readonly IServerManageHub _serverManageHub;

        public event Action ServerNonePlayer;
        public event Action ServerHavePlayerAgain;

        public LivePlayers(SignalRManager signalRManager)
        {
            _serverManageHub = signalRManager.ServerManageHub;

            signalRManager.ModEventHookHub.SavePlayerData += OnSavePlayerData;
            signalRManager.ModEventHookHub.PlayerSpawning += OnPlayerSpawning;
            signalRManager.ModEventHookHub.PlayerDisconnected += OnPlayerDisconnected;

            signalRManager.Connected += OnSignalRManager_Connected;
            signalRManager.Disconnected += OnSignalRManager_Disconnected;
        }

        private void OnSignalRManager_Connected()
        {
            var livePlayers = _serverManageHub.GetLivePlayers().Result;
            foreach (var livePlayer in livePlayers)
            {
                base[livePlayer.EntityId] = livePlayer;
            }

            if(livePlayers.Count > 0)
            {
                ServerHavePlayerAgain?.Invoke();
            }
        }

        private void OnSignalRManager_Disconnected()
        {
            base.Clear();
            ServerNonePlayer?.Invoke();
        }

        IEnumerable<int> ILivePlayers.Keys => Keys;

        IEnumerable<LivePlayer> ILivePlayers.Values => Values;

        LivePlayer ILivePlayers.this[int entityId] 
        {
            get 
            {
                if(TryGetPlayer(entityId, out var livePlayer))
                {
                    return livePlayer;
                }

                throw new KeyNotFoundException("SteamId not found in online players");
            }
        }

        public bool TryGetPlayer(int entityId, out LivePlayer livePlayer)
        {
            if (base.TryGetValue(entityId, out livePlayer) == false)
            {
                livePlayer = _serverManageHub.GetLivePlayer(entityId).Result;
                if(livePlayer == null)
                {
                    return false;
                }
                else
                {
                    // 插入或更新项
                    base[entityId] = livePlayer;
                }
            }

            return true;
        }

        private void OnPlayerDisconnected(int entityId)
        {
            base.TryRemove(entityId, out _);

            if (base.IsEmpty && _serverManageHub.GetLivePlayerCount().Result == 0)
            {
                ServerNonePlayer?.Invoke();
            }
        }

        private void OnSavePlayerData(LivePlayer livePlayer)
        {
            base[livePlayer.EntityId] = livePlayer;
        }

        private void OnPlayerSpawning(LivePlayer livePlayer)
        {
            bool isEmpty = base.IsEmpty;

            base[livePlayer.EntityId] = livePlayer;

            if (isEmpty && _serverManageHub.GetLivePlayerCount().Result > 0)
            {
                ServerHavePlayerAgain?.Invoke();
            }
        }
    }
}
