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
        private IHubProxy _hubProxy;

        public event Action<ChatMessage> ChatMessage;
        public event Action<Entity> EntitySpawned;
        public event Action GameAwake;
        public event Action GameShutdown;
        public event Action GameStartDone;

        public event Action<int> PlayerDisconnected;
        public event Action<LivePlayer> SavePlayerData;
        public event Action<LivePlayer> PlayerSpawning;
        public event Action<PlayerSpawnedEventArgs> PlayerSpawnedInWorld;
        public event Action<Entity, int> EntityKilled;


        public ModEventHookHubReceiver(HubConnection hubConnection)
        {
            _hubProxy = hubConnection.CreateHubProxy(nameof(IModEventHookHub));
            Subscribe(this);
        }

        private void Subscribe(IModEventHookHub hub)
        {
            _hubProxy.On(hub.OnGameAwake);
            _hubProxy.On(hub.OnGameShutdown);
            _hubProxy.On(hub.OnGameStartDone);

            _hubProxy.On<Entity>(hub.OnEntitySpawned);
            _hubProxy.On<ChatMessage>(hub.OnChatMessage);
            _hubProxy.On<int>(hub.OnPlayerDisconnected);
            _hubProxy.On<LivePlayer>(hub.OnSavePlayerData);
            _hubProxy.On<LivePlayer>(hub.OnPlayerSpawning);
            _hubProxy.On<PlayerSpawnedEventArgs>(hub.OnPlayerSpawnedInWorld);
            _hubProxy.On<Entity, int>(hub.OnEntityKilled);
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

        void IModEventHookHub.OnEntitySpawned(Entity entity)
        {
            EntitySpawned?.Invoke(entity);
        }
        void IModEventHookHub.OnChatMessage(ChatMessage chatMessage)
        {
            ChatMessage?.Invoke(chatMessage);
        }

        void IModEventHookHub.OnPlayerDisconnected(int entityId)
        {
            PlayerDisconnected?.Invoke(entityId);
        }

        void IModEventHookHub.OnSavePlayerData(LivePlayer livePlayer)
        {
            SavePlayerData?.Invoke(livePlayer);
        }

        void IModEventHookHub.OnPlayerSpawning(LivePlayer livePlayer)
        {
            PlayerSpawning?.Invoke(livePlayer);
        }

        void IModEventHookHub.OnPlayerSpawnedInWorld(PlayerSpawnedEventArgs playerSpawnedEventArgs)
        {
            PlayerSpawnedInWorld?.Invoke(playerSpawnedEventArgs);
        }

        void IModEventHookHub.OnEntityKilled(Entity entity, int entityIdThatKilledMe)
        {
            EntityKilled?.Invoke(entity, entityIdThatKilledMe);
        }
    }
}
