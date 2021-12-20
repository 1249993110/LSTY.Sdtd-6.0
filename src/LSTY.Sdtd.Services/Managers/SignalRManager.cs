using LSTY.Sdtd.Services.HubReceivers;
using LSTY.Sdtd.Services.Models;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LSTY.Sdtd.Services.Managers
{
    public class SignalRManager
    {
        private readonly ILogger<SignalRManager> _logger;
        private readonly FunctionSettings _functionSettings;
        private HubConnection _hubConnection;

        public SignalRManager(ILogger<SignalRManager> logger, IOptionsMonitor<FunctionSettings> optionsMonitor)
        {
            _logger = logger;
            _functionSettings = optionsMonitor.CurrentValue;
        }

        public ServerManageHubReceiver ServerManageHub {  get; private set; }
        public ModEventHookHubReceiver ModEventHookHub {  get; private set; }

        public event Action Disconnected;

        public event Action Connected;

        public async Task ConnectAsync()
        {
            string signalRUrl = _functionSettings.SignalRUrl;
            string accessToken = _functionSettings.SignalRAccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                _hubConnection = new HubConnection(signalRUrl);
            }
            else
            {
                _hubConnection = new HubConnection(signalRUrl, "?access-token=" + accessToken);
            }

            _hubConnection.Error += OnConnectionError;
            _hubConnection.Closed += OnConnectionClosed;

            ServerManageHub = new ServerManageHubReceiver(_hubConnection);
            ModEventHookHub = new ModEventHookHubReceiver(_hubConnection);

            await Start();
        }

        private async Task Start()
        {
            try
            {
                if (_hubConnection.State == ConnectionState.Disconnected)
                {
                    await _hubConnection.Start();
                    Connected?.Invoke();
                    _logger.LogInformation("Successful connection with Signal server, server address: " + _hubConnection.Url);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to the Signal server, please confirm whether the server is started");
            }
        }

        private void OnConnectionError(Exception ex)
        {
            _logger.LogError(ex, "Error in signalR connection");
        }

        private void OnConnectionClosed()
        {
            Disconnected?.Invoke();

            _logger.LogInformation("SignalR connection closed, prepare to try reconnect");
            Task.Delay(5000).ContinueWith((task, state) => { ((SignalRManager)state).Start().Wait(); }, this);
        }
    }
}