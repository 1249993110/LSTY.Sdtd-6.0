using IceCoffee.Common.Timers;
using LSTY.Sdtd.Services.Models.Configs;
using LSTY.Sdtd.Shared.Models;
using Microsoft.Extensions.Logging;

namespace LSTY.Sdtd.Services.Functions
{
    public class GameNotice : FunctionBase
    {
        private SubTimer _timer;
        private readonly ILogger<GameNotice> _logger;

        public GameNotice(ILogger<GameNotice> logger)
        {
            this._logger = logger;
            _timer = new SubTimer(SendAlternateNotice, Config.AlternateInterval);
        }

        protected override void OnConfigChanged()
        {
            _timer.Interval = Config.AlternateInterval;
        }

        private void SendAlternateNotice()
        {
            if (Config.AlternateNotices.Length > 0)
            {
                Random rd = new Random();
                int index = rd.Next(Config.AlternateNotices.Length);
                SendGlobalMessage(Config.AlternateNotices[index]);
            }
        }

        protected override void DisableFunction()
        {
            _timer.IsEnabled = false;
            GlobalTimer.UnregisterSubTimer(_timer);
            ModEventHookHubReceiver.PlayerSpawnedInWorld -= PlayerSpawnedInWorld;
        }

        protected override void EnableFunction()
        {
            _timer.IsEnabled = true;
            GlobalTimer.RegisterSubTimer(_timer);
            ModEventHookHubReceiver.PlayerSpawnedInWorld += PlayerSpawnedInWorld;
        }

        private void PlayerSpawnedInWorld(PlayerSpawnedEventArgs playerSpawnedEventArgs)
        {
            try
            {
                switch (playerSpawnedEventArgs.RespawnType)
                {
                    // New player spawning
                    case RespawnType.EnterMultiplayer:
                    // Old player spawning
                    case RespawnType.JoinMultiplayer:
                        SendMessageToPlayer(playerSpawnedEventArgs.EntityId, FormatCmd(Config.WelcomeNotice, OnlinePlayers[playerSpawnedEventArgs.SteamId]));
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error in GameNotice.PlayerSpawnedInWorld");
            }
        }
    }
}