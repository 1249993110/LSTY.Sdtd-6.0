using IceCoffee.Common.Templates;
using LSTY.Sdtd.Data.Entities;
using LSTY.Sdtd.Data.IRepositories;
using LSTY.Sdtd.Services.Extensions;
using LSTY.Sdtd.Services.HubReceivers;
using LSTY.Sdtd.Services.Internal;
using LSTY.Sdtd.Services.Models;
using LSTY.Sdtd.Services.Models.Configs;
using LSTY.Sdtd.Shared;
using LSTY.Sdtd.Shared.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

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

        private static ILogger<FunctionBase> _logger;
        private static BasicConfig _basicConfig;
        private static ModEventHookHubReceiver _modEventHookHubReceiver;
        private static ServerManageHubReceiver _serverManageHubReceiver;
        private static IOnlinePlayers _onlinePlayers;

        protected static ModEventHookHubReceiver ModEventHookHubReceiver => _modEventHookHubReceiver;
        protected static ServerManageHubReceiver ServerManageHubReceiver => _serverManageHubReceiver;
        protected static IOnlinePlayers OnlinePlayers => _onlinePlayers;

        /// <summary>
        /// 静态注入并初始化
        /// </summary>
        internal static void InjectAndInit(
            ILogger<FunctionBase> logger,
            IChatLogRepository chatLogRepository, 
            ModEventHookHubReceiver modEventHookHubReceiver,
            ServerManageHubReceiver serverManageHubReceiver,
            IOnlinePlayers onlinePlayers)
        {
            _logger = logger;

            _modEventHookHubReceiver = modEventHookHubReceiver;
            _serverManageHubReceiver = serverManageHubReceiver;
            _onlinePlayers = onlinePlayers;
        }

        protected abstract void OnConfigChanged(FunctionConfig functionConfigs, string name);

        protected static void SendGlobalMessage(string message)
        {
            _serverManageHubReceiver.SendGlobalMessage(message, _basicConfig.ServerName).Wait();
        }

        protected static void SendMessageToPlayer(string playerId, string message)
        {
            _serverManageHubReceiver.SendMessageToPlayer(playerId, message, _basicConfig.ServerName).Wait();
        }

        public FunctionBase()
        {
            _functionName = this.GetType().Name;
            _onlinePlayers.ServerNonePlayer += PrivateDisableFunction;
            _onlinePlayers.ServerHavePlayerAgain += PrivateEnableFunction;
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
                    if (OnlinePlayers.IsEmpty == false)
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
        /// Enabled function, regardless of whether there are players on the server , return value will set to _isRunning
        /// </summary>
        protected virtual bool EnableFunctionNonePlayer()
        {
            return false;
        }
       

        /// <summary>
        /// Call when capturing player chat message, return true mean this message were handled by current function
        /// </summary>
        protected virtual bool OnPlayerChatHooked(OnlinePlayer onlinePlayer, string message)
        {
            return false;
        }

        private static bool HandleChatMessage(ChatHook chatHook, string steamId, string message)
        {
            try
            {
                return chatHook.Invoke(_onlinePlayers[steamId], message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in chathook");
                SendMessageToPlayer(steamId, _basicConfig.HandleChatMessageErrorTips);
                return false;
            }
        }

    }
}
