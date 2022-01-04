using IceCoffee.DbCore.Primitives.Repository;
using LSTY.Sdtd.Data.Dtos;
using LSTY.Sdtd.Data.Entities;
using System;
using System.Collections.Generic;

namespace LSTY.Sdtd.Data.IRepositories
{
    public interface IVChatRecordRepository : IRepository<V_ChatRecord>
    {
        IEnumerable<V_ChatRecord> QueryByDateTime(DateTime startDateTime, DateTime endDateTime, string orderBy = null);

        IEnumerable<V_ChatRecord> QueryBySenderName(string senderName, bool fuzzySearch = false);

        IEnumerable<V_ChatRecord> QueryByEntityId(int entityId);

        IEnumerable<V_ChatRecord> QueryByPlatformUserId(string platformUserId);

        IEnumerable<V_ChatRecord> QueryByEOS(string EOS);

        Task<PaginationResultDto> QueryPagedAsync(ChatRecordQueryDto dto);
    }
}
