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
                    LandProtectionMultiplier = landProtection.Item2
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

        
    }
}
