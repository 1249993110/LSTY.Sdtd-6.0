﻿using LSTY.Sdtd.Data.IRepositories;
using LSTY.Sdtd.Services.Models;
using LSTY.Sdtd.Services.Extensions;
using LSTY.Sdtd.Services.HubReceivers;
using LSTY.Sdtd.Services.Managers;
using LSTY.Sdtd.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LSTY.Sdtd.Services.Functions
{
    public abstract class FunctionBase
    {
        private string _functionName;
        private bool _isEnabled;
        private bool _isRunning;
        public string FunctionName => _functionName;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (value)
                {
                    _isEnabled = value;
                    PrivateEnableFunction();
                }
                else
                {
                    _isEnabled = value;
                    PrivateDisableFunction();
                }
            }
        }

        private readonly IOptionsMonitor<FunctionSettings> _optionsMonitor;

        protected readonly ILogger Logger;
        protected FunctionSettings FunctionSettings => _optionsMonitor.CurrentValue;
        protected readonly ModEventHookHubReceiver ModEventHookHub;
        protected readonly ServerManageHubReceiver ServerManageHub;
        protected readonly ILivePlayers LivePlayers;


        public FunctionBase(ILoggerFactory loggerFactory, IOptionsMonitor<FunctionSettings> optionsMonitor, SignalRManager signalRManager, ILivePlayers livePlayers)
        {
            _functionName = this.GetType().Name;
            Logger = loggerFactory.CreateLogger(this.GetType());

            _optionsMonitor = optionsMonitor;
            _optionsMonitor.OnChange(OnSettingsChanged);

            ModEventHookHub = signalRManager.ModEventHookHub;
            ServerManageHub = signalRManager.ServerManageHub;
            LivePlayers = livePlayers;
            LivePlayers.ServerNonePlayer += PrivateDisableFunction;
            LivePlayers.ServerHavePlayerAgain += PrivateEnableFunction;
        }

        protected abstract void OnSettingsChanged(FunctionSettings settings);

        protected void SendGlobalMessage(string message)
        {
            ServerManageHub.SendGlobalMessage(message, FunctionSettings.ServerName).Wait();
        }

        protected void SendMessageToPlayer(object playerIdOrName, string message)
        {
            ServerManageHub.SendMessageToPlayer(playerIdOrName, message, FunctionSettings.ServerName).Wait();
        }

        /// <summary>
        /// Prevent duplicate settings
        /// </summary>
        private void PrivateDisableFunction()
        {
            lock (this)
            {
                // If the function is not disabled
                if (_isRunning)
                {
                    _isRunning = false;
                    DisableFunction();
                }
            }
        }

        /// <summary>
        /// Prevent duplicate settings
        /// </summary>
        private void PrivateEnableFunction()
        {
            lock (this)
            {
                // If the function is not running
                if (_isRunning == false && _isEnabled)
                {
                    _isRunning = EnableFunctionNonePlayer();

                    // only there are players on the server
                    if (LivePlayers.IsEmpty == false)
                    {
                        _isRunning = true;
                        EnableFunction();
                    }
                }
            }
        }

        /// <summary>
        /// Disabled function, the default implementation ChatHook of the base class
        /// </summary>
        protected virtual void DisableFunction()
        {
        }

        /// <summary>
        /// Enabled function, the default implementation ChatHook of the base class
        /// </summary>
        protected virtual void EnableFunction()
        {
        }

        /// <summary>
        /// Enabled function, 无论服务器上是否有玩家, 返回值将设置到 _isRunning
        /// </summary>
        protected virtual bool EnableFunctionNonePlayer()
        {
            return false;
        }

        protected virtual string FormatCmd(string message, PlayerBase player)
        {
            if (string.IsNullOrEmpty(message))
            {
                return string.Empty;
            }

            if (player == null)
            {
                return message;
            }

            return IceCoffee.Common.Templates.StringTemplate.Render(message, player);
        }
    }
}