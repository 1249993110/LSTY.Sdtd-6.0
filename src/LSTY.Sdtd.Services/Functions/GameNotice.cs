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
        private GameNoticeSettings _settings;

        public GameNotice(ILoggerFactory loggerFactory, IOptionsMonitor<FunctionSettings> optionsMonitor, SignalRManager signalRManager, ILivePlayers livePlayers)
            : base(loggerFactory, optionsMonitor, signalRManager, livePlayers)
        {
            _timer = new SubTimer(SendAlternateNotice);
            OnSettingsChanged(FunctionSettings);
        }

        protected override void OnSettingsChanged(FunctionSettings settings)
        {
            _settings = settings.GameNoticeSettings;
            base.IsEnabled = _settings.IsEnabled;
            _timer.Interval = _settings.AlternateInterval;
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

        protected override void OnDisableFunction()
        {
            _timer.IsEnabled = false;
            GlobalTimer.UnregisterSubTimer(_timer);
            ModEventHookHub.PlayerSpawnedInWorld -= PlayerSpawnedInWorld;
        }

        protected override void OnEnableFunction()
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
                        SendMessageToPlayer(playerSpawnedEventArgs.EntityId, FormatCmd(_settings.WelcomeNotice, playerSpawnedEventArgs));
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