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

        public PersistentManager(ILogger<PersistentManager> logger,
            SignalRManager signalRManager,
            IPlayerRepository playerRepository,
            IInventoryRepository inventoryRepository,
            IChatRecordRepository chatRecordRepository, 
            ILivePlayers livePlayers)
        {
            _logger = logger;
            _serverManageHub = signalRManager.ServerManageHub;
            signalRManager.ModEventHookHub.ChatMessage += OnChatMessage;
            _playerRepository = playerRepository;
            _inventoryRepository = inventoryRepository;
            _chatRecordRepository = chatRecordRepository;
            livePlayers.PlayerUpdate += SavePlayerData;
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
                    LastPositionX = livePlayer.Position.X,
                    LastPositionY = livePlayer.Position.Y,
                    LastPositionZ = livePlayer.Position.Z,
                    Name = livePlayer.Name,
                    Level = livePlayer.Level,
                    Health = livePlayer.Health,
                    Stamina = livePlayer.Stamina,
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
        
        private async Task SaveInventory()
        {
            try
            {
                var playerInventories = await _serverManageHub.GetPlayerInventories();
                foreach (var playerInventory in playerInventories)
                {
                    try
                    {
                        if (playerInventory.Inventory != null)
                        {
                            var serializedContent = JsonSerializer.Serialize(playerInventory.Inventory, new JsonSerializerOptions()
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });

                            _inventoryRepository.ReplaceInto(new Data.Entities.T_Inventory()
                            {
                                EntityId = playerInventory.EntityId,
                                SerializedContent = serializedContent
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in PersistentManager.SaveInventory:foreach");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PersistentManager.SaveInventory");
            }
        }

        public void SavePlayerData(IEnumerable<LivePlayer> livePlayers)
        {
            // 保存玩家背包
            Task.Run(SaveInventory);

            // 保存玩家
            foreach (var livePlayer in livePlayers)
            {
                SaveLivePlayer(livePlayer);
            }
        }
    }
}
