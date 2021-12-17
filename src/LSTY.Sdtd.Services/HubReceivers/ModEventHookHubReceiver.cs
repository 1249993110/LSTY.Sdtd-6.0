using LSTY.Sdtd.Shared;
using LSTY.Sdtd.Shared.Hubs;
using LSTY.Sdtd.Shared.Models;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text;
using LSTY.Sdtd.Services.Extensions;
using System.Collections.Concurrent;

namespace LSTY.Sdtd.Services.HubReceivers
{
    public class ModEventHookHubReceiver : IModEventHookHub
    {
        private readonly SignalRManager _signalRManager;
        private IHubProxy _hubProxy;

        public event Action<ChatMessage> ChatMessage;
        public event Action<Entity> EntitySpawned;
        public event Action GameAwake;
        public event Action GameShutdown;
        public event Action GameStartDone;

        public event Action<string> PlayerDisconnected;
        public event Action<OnlinePlayer> SavePlayerData;
        public event Action<OnlinePlayer> PlayerSpawning;
        public event Action<PlayerSpawnedEventArgs> PlayerSpawnedInWorld;
        public event Action<Entity, string> EntityKilled;


        public ModEventHookHubReceiver(SignalRManager signalRManager)
        {
            _signalRManager = signalRManager;
            _signalRManager.Ready += OnSignalRManager_Ready;
        }

        private void OnSignalRManager_Ready(HubConnection hubConnection)
        {
            _hubProxy = hubConnection.CreateHubProxy(nameof(IModEventHookHub));
            Subscribe(this);
        }

        private void Subscribe(IModEventHookHub hub)
        {
            _hubProxy.On<ChatMessage>(hub.OnChatMessage);
            _hubProxy.On<Entity>(hub.OnEntitySpawned);
            _hubProxy.On(hub.OnGameAwake);
            _hubProxy.On(hub.OnGameShutdown);
            _hubProxy.On(hub.OnGameStartDone);

            _hubProxy.On<string>(hub.OnPlayerDisconnected);
            _hubProxy.On<OnlinePlayer>(hub.OnSavePlayerData);
            _hubProxy.On<OnlinePlayer>(hub.OnPlayerSpawning);
            _hubProxy.On<PlayerSpawnedEventArgs>(hub.OnPlayerSpawnedInWorld);
            _hubProxy.On<Entity, string>(hub.OnEntityKilled);
        }

        void IModEventHookHub.OnChatMessage(ChatMessage chatMessage)
        {
            ChatMessage?.Invoke(chatMessage);
        }

        void IModEventHookHub.OnEntitySpawned(Entity entity)
        {
            EntitySpawned?.Invoke(entity);
        }

        void IModEventHookHub.OnGameAwake()
        {
            GameAwake?.Invoke();
        }

        void IModEventHookHub.OnGameShutdown()
        {
            GameShutdown?.Invoke();
        }

        void IModEventHookHub.OnGameStartDone()
        {
            GameStartDone?.Invoke();
        }

        void IModEventHookHub.OnPlayerDisconnected(string steamId)
        {
            PlayerDisconnected?.Invoke(steamId);
        }

        void IModEventHookHub.OnSavePlayerData(OnlinePlayer onlinePlayer)
        {
            SavePlayerData?.Invoke(onlinePlayer);
        }

        void IModEventHookHub.OnPlayerSpawning(OnlinePlayer onlinePlayer)
        {
            PlayerSpawning?.Invoke(onlinePlayer);
        }

        void IModEventHookHub.OnPlayerSpawnedInWorld(PlayerSpawnedEventArgs playerSpawnedEventArgs)
        {
            PlayerSpawnedInWorld?.Invoke(playerSpawnedEventArgs);
        }

        void IModEventHookHub.OnEntityKilled(Entity entity, string steamIdThatKilledMe)
        {
            EntityKilled?.Invoke(entity, steamIdThatKilledMe);
        }
    }
}
