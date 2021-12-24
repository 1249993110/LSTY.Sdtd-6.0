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
    /// <summary>
    /// 为避免死锁，应将Receiver结果分派给其他线程
    /// </summary>
    /// <remarks>
    /// https://github.com/SignalR/SignalR/issues/3895
    /// </remarks>
    public class ModEventHookHubReceiver : IModEventHookHub
    {
        private IHubProxy _hubProxy;

        public event Action<LogEntry> LogCallback;
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
            _hubProxy.On<LogEntry>(hub.OnLogCallback);
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

        void IModEventHookHub.OnLogCallback(LogEntry logEntry)
        {
            if(LogCallback != null)
            {
                Task.Run(() => LogCallback.Invoke(logEntry));
            }
        }

        void IModEventHookHub.OnGameAwake()
        {
            if (GameAwake != null)
            {
                Task.Run(GameAwake.Invoke);
            }
        }

        void IModEventHookHub.OnGameShutdown()
        {
            if (GameShutdown != null)
            {
                Task.Run(GameShutdown.Invoke);
            }
        }

        void IModEventHookHub.OnGameStartDone()
        {
            if (GameStartDone != null)
            {
                Task.Run(GameStartDone.Invoke);
            }
        }

        void IModEventHookHub.OnEntitySpawned(Entity entity)
        {
            if (EntitySpawned != null)
            {
                Task.Run(() => EntitySpawned.Invoke(entity));
            }
        }
        void IModEventHookHub.OnChatMessage(ChatMessage chatMessage)
        {
            if (ChatMessage != null)
            {
                Task.Run(() => ChatMessage.Invoke(chatMessage));
            }
        }

        void IModEventHookHub.OnPlayerDisconnected(int entityId)
        {
            if (PlayerDisconnected != null)
            {
                Task.Run(() => PlayerDisconnected.Invoke(entityId));
            }
        }

        void IModEventHookHub.OnSavePlayerData(LivePlayer livePlayer)
        {
            if (SavePlayerData != null)
            {
                Task.Run(() => SavePlayerData.Invoke(livePlayer));
            }
        }

        void IModEventHookHub.OnPlayerSpawning(LivePlayer livePlayer)
        {
            if (PlayerSpawning != null)
            {
                Task.Run(() => PlayerSpawning.Invoke(livePlayer));
            }
        }

        void IModEventHookHub.OnPlayerSpawnedInWorld(PlayerSpawnedEventArgs playerSpawnedEventArgs)
        {
            if (PlayerSpawnedInWorld != null)
            {
                Task.Run(() => PlayerSpawnedInWorld.Invoke(playerSpawnedEventArgs));
            }
        }

        void IModEventHookHub.OnEntityKilled(Entity entity, int entityIdThatKilledMe)
        {
            if (EntityKilled != null)
            {
                Task.Run(() => EntityKilled.Invoke(entity, entityIdThatKilledMe));
            }
        }
    }
}
