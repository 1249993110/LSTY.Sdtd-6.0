using LSTY.Sdtd.PatronsMod.Internal;
using LSTY.Sdtd.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.PatronsMod.Extensions
{
    internal static class ClientInfoExtension
    {
        public static LivePlayer ToLivePlayer(this ClientInfo clientInfo)
        {
            try
            {
                PlayerDataFile pdf = clientInfo.latestPlayerData;
                Progression progression = GetProgression(pdf);
                var landProtection = GetLandProtectionActiveAndMultiplier(clientInfo.entityId);

                return new LivePlayer()
                {
                    PlatformUserId = clientInfo.PlatformId.ReadablePlatformUserIdentifier,
                    PlatformType = clientInfo.PlatformId.PlatformIdentifierString,
                    EntityId = clientInfo.entityId,
                    Name = clientInfo.playerName,
                    IP = clientInfo.ip,
                    ExpToNextLevel = progression == null ? 0 : progression.ExpToNextLevel,
                    Level = progression == null ? 0 : progression.Level,
                    Ping = clientInfo.ping,
                    CurrentLife = pdf.currentLife,
                    TotalPlayTime = pdf.totalTimePlayed,
                    LastPosition = GetLastPosition(clientInfo.entityId),
                    Score = pdf.score,
                    ZombieKills = pdf.zombieKills,
                    PlayerKills = pdf.playerKills,
                    Deaths = pdf.deaths,
                    LandProtectionActive = landProtection.Item1,
                    LandProtectionMultiplier = landProtection.Item2,
                    Inventory = GetInventory(pdf)
                };
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "ClientInfo to LivePlayer failed");
                return null;
            }
        }

        private static Progression GetProgression(PlayerDataFile pdf)
        {
            try
            {
                if (pdf.progressionData.Length <= 0)
                {
                    return null;
                }

                using (PooledBinaryReader pbr = MemoryPools.poolBinaryReader.AllocSync(false))
                {
                    pbr.SetBaseStream(pdf.progressionData);
                    long posBefore = pbr.BaseStream.Position;
                    pbr.BaseStream.Position = 0;
                    Progression p = Progression.Read(pbr, null);
                    pbr.BaseStream.Position = posBefore;

                    return p;
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "Get Progression from PlayerDataFile failed");
                return null;
            }
        }

        private static Position GetLastPosition(int entityId)
        {
            try
            {
                return GameManager.Instance.World.Players.dict[entityId].GetPosition().ToPosition();
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "Get player last position failed, it may be because the player is joining the game, and will return -1 -1 -1");
                return new Position(-1, -1, -1);
            }
        }

        private static Tuple<bool, float> GetLandProtectionActiveAndMultiplier(int entityId)
        {
            try
            {
                var world = GameManager.Instance.World;
                var playerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(entityId);

                return new Tuple<bool, float>(world.IsLandProtectionValidForPlayer(playerData),
                    world.GetLandProtectionHardnessModifierForPlayer(playerData));
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "Get player land protection state failed");
                return new Tuple<bool, float>(default, default);
            }
        }

        #region Get Inventory
        private static Shared.Models.Inventory GetInventory(PlayerDataFile pdf)
        {
            try
            {
                return new Shared.Models.Inventory()
                {
                    Bag = ProcessInv(pdf.bag, pdf.id),
                    Belt = ProcessInv(pdf.inventory, pdf.id),
                    Equipment = ProcessEqu(pdf.equipment, pdf.id)
                };
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "Get player inventory from PlayerDataFile failed");
                return null;
            }
        }

        private static List<InvItem> ProcessInv(ItemStack[] sourceFields, int entityId)
        {
            var target = new List<InvItem>(sourceFields.Length);

            foreach (var field in sourceFields)
            {
                InvItem invItem = CreateInvItem(field.itemValue, field.count, entityId);
                if (invItem != null && field.itemValue.Modifications != null)
                {
                    ProcessParts(field.itemValue.Modifications, invItem, entityId);
                }

                target.Add(invItem);
            }

            return target;
        }

        private static InvItem[] ProcessEqu(Equipment sourceEquipment, int entityId)
        {
            int slotCount = sourceEquipment.GetSlotCount();
            var equipment = new InvItem[slotCount];
            for (int i = 0; i < slotCount; ++i)
            {
                equipment[i] = CreateInvItem(sourceEquipment.GetSlotItem(i), 1, entityId);
            }

            return equipment;
        }

        private static void ProcessParts(ItemValue[] parts, InvItem item, int entityId)
        {
            int length = parts.Length;

            InvItem[] itemParts = new InvItem[length];

            for (int i = 0; i < length; ++i)
            {
                InvItem partItem = CreateInvItem(parts[i], 1, entityId);
                if (partItem != null && parts[i].Modifications != null)
                {
                    ProcessParts(parts[i].Modifications, partItem, entityId);
                }

                itemParts[i] = partItem;
            }

            item.Parts = itemParts;
        }

        private static InvItem CreateInvItem(ItemValue itemValue, int count, int entityId)
        {
            if (count <= 0 || itemValue == null || itemValue.Equals(ItemValue.None))
            {
                return null;
            }

            ItemClass itemClass = ItemClass.list[itemValue.type];
            //int maxAllowed = itemClass.Stacknumber.Value;
            string name = itemClass.GetItemName();

            //string steamId = ConnectionManager.Instance.Clients.ForEntityId(entityId).playerId;

            //var inventoryCheck = FunctionManager.AntiCheat.InventoryCheck;
            //if (inventoryCheck.IsEnabled)
            //{
            //    inventoryCheck.Execute(steamId, name, count, maxAllowed);
            //}

            int quality = itemValue.HasQuality ? itemValue.Quality : -1;

            InvItem item = new InvItem()
            {
                ItemName = name,
                Count = count,
                Quality = quality,
                Icon = itemClass.GetIconName(),
                Iconcolor = itemClass.GetIconTint().ToHex(),
                MaxUseTimes = itemValue.MaxUseTimes,
                UseTimes = itemValue.UseTimes
            };

            return item;
        }
        #endregion
    }
}
