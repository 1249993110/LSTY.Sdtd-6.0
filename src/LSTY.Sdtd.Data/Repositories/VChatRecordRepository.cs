using IceCoffee.DbCore.ExceptionCatch;
using IceCoffee.DbCore.Primitives.Repository;
using LSTY.Sdtd.Data.Entities;
using LSTY.Sdtd.Data.IRepositories;
using LSTY.Sdtd.Data;
using System;
using System.Collections.Generic;
using LSTY.Sdtd.Data.Dtos;

namespace LSTY.Sdtd.Data.Repositories
{
    public class VChatRecordRepository : SQLiteRepository<V_ChatRecord>, IVChatRecordRepository
    {
        public VChatRecordRepository(DefaultDbConnectionInfo dbConnectionInfo) : base(dbConnectionInfo)
        {
        }

        public IEnumerable<V_ChatRecord> QueryByDateTime(DateTime startDateTime, DateTime endDateTime, string orderBy = null)
        {
            string whereBy = "CreatedDate BETWEEN @StartDateTime AND @EndDateTime";
            return base.Query(whereBy, orderBy, new { StartDateTime = startDateTime, EndDateTime = endDateTime });
        }

        public IEnumerable<V_ChatRecord> QueryBySenderName(string senderName, bool fuzzySearch = false)
        {
            if (fuzzySearch == false)
            {
                return base.QueryById(nameof(V_ChatRecord.SenderName), senderName);
            }
            else
            {
                return base.Query("PlayerName Name LIKE '%'||@Keyword||'%'", "CreatedDate DESC", new { Keyword = senderName });
            }
        }

        public IEnumerable<V_ChatRecord> QueryByEntityId(int entityId)
        {
            return base.QueryById(nameof(V_ChatRecord.EntityId), entityId);

        }

        public IEnumerable<V_ChatRecord> QueryByPlatformUserId(string platformUserId)
        {
            return base.QueryById(nameof(V_ChatRecord.PlatformUserId), platformUserId);

        }

        public IEnumerable<V_ChatRecord> QueryByEOS(string EOS)
        {
            return base.QueryById(nameof(V_ChatRecord.EOS), EOS);

        }

        public async Task<PaginationResultDto> QueryPagedAsync(ChatRecordQueryDto dto)
        {
            string orderBy = null;

            if (string.IsNullOrEmpty(dto.Order) == false && typeof(V_ChatRecord).GetProperty(dto.Order) != null)// 避免sql注入
            {
                orderBy = dto.Order + (dto.Desc ? "DESC" : "ASC");
            }
            else
            {
                orderBy = "CreatedDate " + (dto.Desc ? "DESC" : "ASC");
            }

            string whereBy = null;

            if(dto.StartDateTime.HasValue && dto.EndDateTime.HasValue)
            {
                whereBy = "(CreatedDate BETWEEN @StartDateTime AND @EndDateTime)";
            }

            if(string.IsNullOrEmpty(dto.IdOrName) == false)
            {
                if(whereBy != null)
                {
                    whereBy += " AND ";
                }

                whereBy += "(EntityId=@IdOrName OR PlatformUserId=@IdOrName OR EOS=@IdOrName OR SenderName LIKE '%'||@IdOrName||'%')";
            }

            var items = await base.QueryPagedAsync(dto.PageIndex, dto.PageSize, whereBy, orderBy, dto);
            uint total = await base.QueryRecordCountAsync(whereBy, dto);

            return new PaginationResultDto() { Items = items, Total = total };
        }
    }
}
