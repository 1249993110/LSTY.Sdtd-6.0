using IceCoffee.Common.Timers;
using LSTY.Sdtd.Services.Models;
using LSTY.Sdtd.Services.Managers;
using LSTY.Sdtd.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LSTY.Sdtd.Services.Functions
{
    public class GameNotice : FunctionBase
    {
        private readonly SubTimer _timer;
        private readonly GameNoticeSettings _settings;

        public GameNotice(ILoggerFactory loggerFactory, IOptionsMonitor<FunctionSettings> optionsMonitor, SignalRManager signalRManager, ILivePlayers livePlayers)
            : base(loggerFactory, optionsMonitor, signalRManager, livePlayers)
        {
            _settings = FunctionSettings.GameNoticeSettings;
            _timer = new SubTimer(SendAlternateNotice, _settings.AlternateInterval);
        }

        protected override void OnSettingsChanged(FunctionSettings settings)
        {
            _timer.Interval = settings.GameNoticeSettings.AlternateInterval;
        }

        private void SendAlternateNotice()
        {
            var alternateNotices = _settings.AlternateNotices;
            if (alternateNotices.Length > 0)
            {
                Random rd = new Random();
                int index = rd.Next(alternateNotices.Length);
                SendGlobalMessage(alternateNotices[index]);
            }
        }

        protected override void DisableFunction()
        {
            _timer.IsEnabled = false;
            GlobalTimer.UnregisterSubTimer(_timer);
            ModEventHookHub.PlayerSpawnedInWorld -= PlayerSpawnedInWorld;
        }

        protected override void EnableFunction()
        {
            _timer.IsEnabled = true;
            GlobalTimer.RegisterSubTimer(_timer);
            ModEventHookHub.PlayerSpawnedInWorld += PlayerSpawnedInWorld;
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
                        SendMessageToPlayer(playerSpawnedEventArgs.EntityId, 
                            FormatCmd(_settings.WelcomeNotice, LivePlayers[playerSpawnedEventArgs.EntityId]));
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error in GameNotice.PlayerSpawnedInWorld");
            }
        }
    }
}