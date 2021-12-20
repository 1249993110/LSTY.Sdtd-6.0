using LSTY.Sdtd.PatronsMod.Extensions;
using LSTY.Sdtd.Shared;
using LSTY.Sdtd.Shared.Hubs;
using LSTY.Sdtd.Shared.Models;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LSTY.Sdtd.PatronsMod.Internal
{
    static class ModEventHook
    {
        private static IModEventHookHub _hub;

        static ModEventHook()
        {
            try
            {
                _hub = GlobalHost.ConnectionManager.GetHubContext<ModEventHookHub, IModEventHookHub>().Clients.All;
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex.ToString());
            }
        }

        public static void GameAwake()
        {
            Task.Run(_hub.OnGameAwake);
        }

        public static void GameStartDone()
        {
            Task.Run(_hub.OnGameStartDone);
        }

        public static void GameShutdown()
        {
            Task.Run(_hub.OnGameShutdown);
        }

        public static void EntitySpawned(Entity entity)
        {
            Task.Factory.StartNew((state) => 
            {
                if (state is EntityAlive entityAlive)
                {
                    _hub.OnEntitySpawned(new Shared.Models.Entity()
                    {
                        Id = entityAlive.entityId,
                        Name = entityAlive.EntityName,
                        Position = entityAlive.position.ToPosition(),
                        IsPlayer = entityAlive is EntityPlayer
                    });
                }
            }, entity);
        }

        public static void PlayerSpawnedInWorld(ClientInfo clientInfo, RespawnType respawnType, Vector3i position)
        {
            var obj = new PlayerSpawnedEventArgs()
            {
                EntityId = clientInfo.entityId,
                RespawnType = (Shared.Models.RespawnType)respawnType,
                Position = position.ToPosition()
            };

            Task.Factory.StartNew((state) => { _hub.OnPlayerSpawnedInWorld((PlayerSpawnedEventArgs)state); }, obj);
        }

        public static void EntityKilled(Entity killedEntity, Entity entityThatKilledMe)
        {
            if (killedEntity != null
               && entityThatKilledMe != null
               && entityThatKilledMe is EntityPlayer entityPlayer
               && entityThatKilledMe.IsClientControlled())
            {
                int entityIdThatKilledMe = ConnectionManager.Instance.Clients.ForEntityId(entityPlayer.entityId).entityId;

                Shared.Models.Entity entity = null;

                if (killedEntity is EntityPlayer diedPlayer && killedEntity.IsClientControlled())
                {
                    entity = new Shared.Models.Entity()
                    {
                        Id = diedPlayer.entityId,
                        Name = diedPlayer.EntityName,
                        Position = diedPlayer.position.ToPosition(),
                        IsPlayer = true
                    };

                }
                else if (killedEntity is EntityAlive diedEntity && killedEntity.IsClientControlled() == false)
                {
                    entity = new Shared.Models.Entity()
                    {
                        Id = diedEntity.entityId,
                        Name = diedEntity.EntityName,
                        Position = diedEntity.position.ToPosition(),
                        IsPlayer = false
                    };
                }
                else
                {
                    return;
                }

                Task.Factory.StartNew((state) =>
                {
                    _hub.OnEntityKilled((Shared.Models.Entity)state, entityIdThatKilledMe);
                }, entity);
            }
        }

        public static bool ChatMessage(ClientInfo clientInfo, EChatType eChatType, int senderId, string message,
            string mainName, bool localizeMain, List<int> recipientEntityIds)
        {
            ChatMessage chatMessage = new ChatMessage()
            {
                ChatType = (ChatType)eChatType,
                EntityId = senderId,
                Message = message,
                PlayerName = clientInfo?.playerName,
            };

            Task.Factory.StartNew((state) =>
            {
                _hub.OnChatMessage((ChatMessage)state);
            }, chatMessage);

            return true;
        }

        public static void PlayerDisconnected(ClientInfo clientInfo, bool shutdown)
        {
            Task.Factory.StartNew((state) =>
            {
                _hub.OnPlayerDisconnected((int)state);
            }, clientInfo.entityId);
        }

        public static void SavePlayerData(ClientInfo clientInfo, PlayerDataFile pdf)
        {
            Task.Factory.StartNew((state) =>
            {
                _hub.OnSavePlayerData(((ClientInfo)state).ToLivePlayer());
            }, clientInfo);
        }

        public static void PlayerSpawning(ClientInfo clientInfo, int chunkViewDim, PlayerProfile playerProfile)
        {
            Task.Factory.StartNew((state) =>
            {
                _hub.OnPlayerSpawning(((ClientInfo)state).ToLivePlayer());
            }, clientInfo);
        }

    }
}
