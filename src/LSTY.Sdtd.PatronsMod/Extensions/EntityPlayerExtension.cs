﻿using LSTY.Sdtd.PatronsMod.Internal;
using LSTY.Sdtd.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.PatronsMod.Extensions
{
    internal static class EntityPlayerExtension
    {
        public static LivePlayer ToLivePlayer(this EntityPlayer player)
        {
            try
            {
                ClientInfo clientInfo = ConnectionManager.Instance.Clients.ForEntityId(player.entityId);
                Progression progression = GetProgression(clientInfo.latestPlayerData);
                var landProtection = GetLandProtectionActiveAndMultiplier(clientInfo.entityId);

                return new LivePlayer()
                {
                    CurrentLife = player.currentLife,
                    Deaths = player.Died,
                    EntityId = player.entityId,
                    EOS = clientInfo.CrossplatformId.ReadablePlatformUserIdentifier,
                    ExpToNextLevel = progression == null ? -1 : progression.ExpToNextLevel,
                    Health = player.Health,
                    IP = clientInfo.ip,
                    LandProtectionActive = landProtection.Item1,
                    LandProtectionMultiplier = landProtection.Item2,
                    Position = player.GetPosition().ToPosition(),
                    Level = progression == null ? -1 : progression.Level,
                    Name = player.EntityName,
                    Ping = clientInfo.ping,
                    PlatformType = clientInfo.PlatformId.PlatformIdentifierString,
                    PlatformUserId = clientInfo.PlatformId.ReadablePlatformUserIdentifier,
                    PlayerKills = player.KilledPlayers,
                    Score = player.Score,
                    Stamina = player.Stamina,
                    TotalPlayTime = player.totalTimePlayed,
                    ZombieKills = player.KilledZombies
                };
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "EntityPlayer to LivePlayer failed");
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
                return new Tuple<bool, float>(false, -1);
            }
        }
    }
}
