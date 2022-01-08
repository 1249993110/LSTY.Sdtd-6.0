using IceCoffee.Common.Timers;
using LSTY.Sdtd.Data.IRepositories;
using LSTY.Sdtd.Services.Managers;
using LSTY.Sdtd.Shared.Hubs;
using LSTY.Sdtd.Shared.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace LSTY.Sdtd.Services
{
    public class LivePlayers : ConcurrentDictionary<int, LivePlayer>, ILivePlayers
    {
        private readonly ILogger<LivePlayers> _logger;
        private readonly IServerManageHub _serverManageHub;
        private readonly SubTimer _timer;
        public event Action ServerNonePlayer;
        public event Action ServerHavePlayerAgain;
        public event Action<IEnumerable<LivePlayer>> PlayerUpdate;

        public LivePlayers(ILogger<LivePlayers> logger, SignalRManager signalRManager)
        {
            _logger = logger;
            _serverManageHub = signalRManager.ServerManageHub;
            _timer = new SubTimer(GetLivePlayersFromServer, 10)
            {
                IsEnabled = true
            };
            GlobalTimer.RegisterSubTimer(_timer);

            signalRManager.ModEventHookHub.PlayerSpawning += OnPlayerSpawning;
            signalRManager.ModEventHookHub.PlayerDisconnected += OnPlayerDisconnected;

            signalRManager.Connected += On_SignalRManager_Connected;
            signalRManager.Disconnected += On_SignalRManager_Disconnected;

            if (signalRManager.IsConnected)
            {
                GetLivePlayersFromServer();
            }
        }

        private void GetLivePlayersFromServer()
        {
            try
            {
                base.Clear();

                var livePlayers = _serverManageHub.GetPlayers().Result;
                foreach (var livePlayer in livePlayers)
                {
                    base.TryAdd(livePlayer.EntityId, livePlayer);
                }

                if (livePlayers.Any())
                {
                    ServerHavePlayerAgain?.Invoke();
                    PlayerUpdate?.Invoke(livePlayers);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LivePlayers.TryGetLivePlayersFromServer");
            }
        }

        private void On_SignalRManager_Connected()
        {
            GetLivePlayersFromServer();
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
                if (TryGetPlayer(entityId, out var livePlayer))
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
                livePlayer = _serverManageHub.GetPlayer(entityId).Result;
                if (livePlayer == null)
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
            try
            {
                base.TryRemove(entityId, out _);

                if (base.IsEmpty && _serverManageHub.GetPlayerCount().Result == 0)
                {
                    ServerNonePlayer?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LivePlayers.OnPlayerDisconnected");
            }
        }

        private void OnPlayerSpawning(PlayerBase playerBase)
        {
            try
            {
                bool isEmpty = base.IsEmpty;

                var livePlayer = _serverManageHub.GetPlayer(playerBase.EntityId).Result;
                base[playerBase.EntityId] = livePlayer;
  
                if (isEmpty)
                {
                    ServerHavePlayerAgain?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LivePlayers.OnPlayerSpawning");
            }
        }
    }
}