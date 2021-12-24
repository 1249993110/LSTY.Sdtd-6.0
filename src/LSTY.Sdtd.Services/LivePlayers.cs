using LSTY.Sdtd.Services.HubReceivers;
using LSTY.Sdtd.Services.Managers;
using LSTY.Sdtd.Shared.Hubs;
using LSTY.Sdtd.Shared.Models;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Services
{
    public class LivePlayers : ConcurrentDictionary<int, LivePlayer>, ILivePlayers
    {
        private readonly ILogger<LivePlayers> _logger;
        private readonly IServerManageHub _serverManageHub;

        public event Action ServerNonePlayer;
        public event Action ServerHavePlayerAgain;

        public LivePlayers(ILogger<LivePlayers> logger, SignalRManager signalRManager)
        {
            _logger = logger;

            _serverManageHub = signalRManager.ServerManageHub;

            signalRManager.ModEventHookHub.SavePlayerData += OnSavePlayerData;
            signalRManager.ModEventHookHub.PlayerSpawning += OnPlayerSpawning;
            signalRManager.ModEventHookHub.PlayerDisconnected += OnPlayerDisconnected;

            signalRManager.Connected += On_SignalRManager_Connected;
            signalRManager.Disconnected += On_SignalRManager_Disconnected;

            if (signalRManager.IsConnected)
            {
                TryGetLivePlayersFromServer();
            }
        }

        private void TryGetLivePlayersFromServer()
        {
            try
            {
                var livePlayers = _serverManageHub.GetLivePlayers().Result;
                foreach (var livePlayer in livePlayers)
                {
                    base[livePlayer.EntityId] = livePlayer;
                }

                if (livePlayers.Count > 0)
                {
                    ServerHavePlayerAgain?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LivePlayers.TryGetLivePlayersFromServer");
            }
        }

        private void On_SignalRManager_Connected()
        {
            TryGetLivePlayersFromServer();
        }

        private void On_SignalRManager_Disconnected()
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

        public bool ContainsPlayer(int entityId)
        {
            return base.ContainsKey(entityId);
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
