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
        public static PlayerBase ToPlayerBase(this ClientInfo clientInfo)
        {
            return new PlayerBase()
            {
                EntityId = clientInfo.entityId,
                EOS = clientInfo.CrossplatformId.ReadablePlatformUserIdentifier,
                Name = clientInfo.playerName,
                PlatformType = clientInfo.PlatformId.PlatformIdentifierString,
                PlatformUserId = clientInfo.PlatformId.ReadablePlatformUserIdentifier,
            };
        }

        public static PlayerSpawnedEventArgs ToPlayerSpawnedEventArgs(this ClientInfo clientInfo, RespawnType respawnType, Vector3i position)
        {
            return new PlayerSpawnedEventArgs()
            {
                EntityId = clientInfo.entityId,
                EOS = clientInfo.CrossplatformId.ReadablePlatformUserIdentifier,
                Name = clientInfo.playerName,
                PlatformType = clientInfo.PlatformId.PlatformIdentifierString,
                PlatformUserId = clientInfo.PlatformId.ReadablePlatformUserIdentifier,
                RespawnType = (Shared.Models.RespawnType)respawnType,
                Position = position.ToPosition()
            };
        }
    }
}
