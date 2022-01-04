using LSTY.Sdtd.Data.IRepositories;
using LSTY.Sdtd.Services.Managers;
using LSTY.Sdtd.Shared.Hubs;
using LSTY.Sdtd.Shared.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace LSTY.Sdtd.Services.Managers
{
    public class PersistentManager
    {
        private readonly ILogger<PersistentManager> _logger;
        private readonly IServerManageHub _serverManageHub;
        private readonly IPlayerRepository _playerRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IChatRecordRepository _chatRecordRepository;
        private readonly ILivePlayers _livePlayers;

        public PersistentManager(ILogger<PersistentManager> logger,
            SignalRManager signalRManager,
            IPlayerRepository playerRepository,
            IInventoryRepository inventoryRepository,
            IChatRecordRepository chatRecordRepository,
            ILivePlayers livePlayers)
        {
            _logger = logger;
            _serverManageHub = signalRManager.ServerManageHub;
            signalRManager.ModEventHookHub.SavePlayerData += OnSavePlayerData;
            signalRManager.ModEventHookHub.ChatMessage += OnChatMessage;
            _playerRepository = playerRepository;
            _inventoryRepository = inventoryRepository;
            _chatRecordRepository = chatRecordRepository;
            _livePlayers = livePlayers;
        }

        private void OnChatMessage(ChatMessage chatMessage)
        {
            try
            {
                _chatRecordRepository.Insert(new Data.Entities.T_ChatRecord()
                {
                    ChatType = (int)chatMessage.ChatType,
                    EntityId = chatMessage.EntityId,
                    Message = chatMessage.Message,
                    SenderName = chatMessage.SenderName,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PersistentManager.OnChatMessage");
            }
        }

        private void SaveLivePlayer(LivePlayer livePlayer)
        {
            try
            {
                _playerRepository.ReplaceInto(new Data.Entities.T_Player()
                {
                    Deaths = livePlayer.Deaths,
                    EntityId = livePlayer.EntityId,
                    IP = livePlayer.IP,
                    LastOnline = DateTime.Now,
                    LastPositionX = livePlayer.LastPosition.X,
                    LastPositionY = livePlayer.LastPosition.Y,
                    LastPositionZ = livePlayer.LastPosition.Z,
                    Name = livePlayer.Name,
                    Level = livePlayer.Level,
                    PlatformType = livePlayer.PlatformType,
                    PlatformUserId = livePlayer.PlatformUserId,
                    PlayerKills = livePlayer.PlayerKills,
                    Score = livePlayer.Score,
                    TotalPlayTime = livePlayer.TotalPlayTime,
                    ZombieKills = livePlayer.ZombieKills,
                    EOS = livePlayer.EOS,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PersistentManager.SaveLivePlayer");
            }
        }
        private void SaveInventory(int entityId)
        {
            try
            {
                var inventory = _serverManageHub.GetLivePlayerInventory(entityId).Result;

                if (inventory == null)
                {
                    return;
                }

                var serializedContent = JsonSerializer.Serialize(inventory, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _inventoryRepository.ReplaceInto(new Data.Entities.T_Inventory()
                {
                    EntityId = entityId,
                    SerializedContent = serializedContent
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PersistentManager.SaveInventory");
            }
        }

        private void OnSavePlayerData(LivePlayer livePlayer)
        {
            if (livePlayer.EntityId < 0 || string.IsNullOrEmpty(livePlayer.IP))
            {
                return;
            }

            SaveLivePlayer(livePlayer);
            SaveInventory(livePlayer.EntityId);
        }
    }
}
