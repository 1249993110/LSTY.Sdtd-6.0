using Microsoft.AspNet.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace LSTY.Sdtd.Services
{
    public class SignalRManager
    {
        private readonly ILogger<SignalRManager> _logger;
        private HubConnection _hubConnection;

        public SignalRManager(ILogger<SignalRManager> logger)
        {
            this._logger = logger;
        }

        public event Action<HubConnection> Ready;

        public event Action Disconnected;

        public event Action Connected;

        public async Task ConnectAsync(string serverUrl)
        {
            _hubConnection = new HubConnection(serverUrl);
            //_hubConnection.Headers.Add("myauthtoken", /* token data */);
            _hubConnection.Error += OnConnectionError;
            _hubConnection.Closed += OnConnectionClosed;

            Ready?.Invoke(_hubConnection);
            // Ready = null;

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