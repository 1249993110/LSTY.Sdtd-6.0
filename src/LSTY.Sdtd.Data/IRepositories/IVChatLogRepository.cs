using IceCoffee.DbCore.Primitives.Repository;
using LSTY.Sdtd.Data.Entities;
using System;
using System.Collections.Generic;

namespace LSTY.Sdtd.Data.IRepositories
{
    public interface IVChatLogRepository : IRepository<V_ChatLog>
    {
        IEnumerable<V_ChatLog> QueryByDateTime(DateTime startDateTime, DateTime endDateTime, string orderBy = null);

        IEnumerable<V_ChatLog> QueryByPlayerName(string playerName, bool fuzzySearch = false);

        IEnumerable<V_ChatLog> QueryByEntityId(int entityId, bool fuzzySearch = false);

        IEnumerable<V_ChatLog> QueryByPlatformUserId(string platformUserId, bool fuzzySearch = false);

    }
}
