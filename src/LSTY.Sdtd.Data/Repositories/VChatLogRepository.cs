using IceCoffee.DbCore.ExceptionCatch;
using IceCoffee.DbCore.Primitives.Repository;
using LSTY.Sdtd.Data.Entities;
using LSTY.Sdtd.Data.IRepositories;
using LSTY.Sdtd.Data;
using System;
using System.Collections.Generic;

namespace LSTY.Sdtd.Data.Repositories
{
    public class VChatLogRepository : SQLiteRepository<V_ChatLog>, IVChatLogRepository
    {
        public VChatLogRepository(DefaultDbConnectionInfo dbConnectionInfo) : base(dbConnectionInfo)
        {
        }

        public IEnumerable<V_ChatLog> QueryByDateTime(DateTime startDateTime, DateTime endDateTime, string orderBy = null)
        {
            string whereBy = "CreatedDate BETWEEN @StartDateTime AND @EndDateTime";
            return base.Query(whereBy, orderBy, new { StartDateTime = startDateTime, EndDateTime = endDateTime });
        }

        public IEnumerable<V_ChatLog> QueryByPlayerName(string playerName, bool fuzzySearch = false)
        {
            if (fuzzySearch == false)
            {
                return base.QueryById(nameof(V_ChatLog.PlayerName), playerName);
            }
            else
            {
                return base.Query("PlayerName LIKE CONCAT('%',@Keyword,'%')", "CreatedDate DESC", new { Keyword = playerName });
            }
        }

        public IEnumerable<V_ChatLog> QueryByEntityId(int entityId, bool fuzzySearch = false)
        {
            if (fuzzySearch == false)
            {
                return base.QueryById(nameof(V_ChatLog.EntityId), entityId);
            }
            else
            {
                return base.Query("EntityId LIKE CONCAT('%',@Keyword,'%')", "CreatedDate DESC", new { Keyword = entityId });
            }
        }

        public IEnumerable<V_ChatLog> QueryByPlatformUserId(string platformUserId, bool fuzzySearch = false)
        {
            if (fuzzySearch == false)
            {
                return base.QueryById(nameof(V_ChatLog.PlatformUserId), platformUserId);
            }
            else
            {
                return base.Query("PlatformUserId LIKE CONCAT('%',@Keyword,'%')", "CreatedDate DESC", new { Keyword = platformUserId });
            }
        }
    }
}
