using IceCoffee.DbCore.Primitives.Repository;
using LSTY.Sdtd.Data.Dtos;
using LSTY.Sdtd.Data.Entities;
using LSTY.Sdtd.Data.IRepositories;
using System;

namespace LSTY.Sdtd.Data.Repositories
{
    public class PlayerRepository : SQLiteRepository<T_Player>, IPlayerRepository
    {
        public PlayerRepository(DefaultDbConnectionInfo dbConnectionInfo) : base(dbConnectionInfo)
        {
        }

        public IEnumerable<T_Player> QueryByPlayerName(string playerName, bool fuzzySearch = false)
        {
            if (fuzzySearch == false)
            {
                return base.QueryById(nameof(T_Player.Name), playerName);
            }
            else
            {
                return base.Query("Name LIKE '%'||@Keyword||'%'", "CreatedDate DESC", new { Keyword = playerName });
            }
        }

        public IEnumerable<T_Player> QueryByEntityId(int entityId)
        {
            return base.QueryById(nameof(T_Player.EntityId), entityId);
        }

        public IEnumerable<T_Player> QueryByPlatformUserId(string platformUserId)
        {
            return base.QueryById(nameof(T_Player.PlatformUserId), platformUserId);
        }

        public IEnumerable<T_Player> QueryByEOS(string EOS)
        {
            return base.QueryById(nameof(T_Player.EOS), EOS);
        }

        public async Task<PaginationResultDto> QueryPagedAsync(PlayersQueryDto dto)
        {
            string orderBy = null;

            if (string.IsNullOrEmpty(dto.Order) == false && typeof(T_Player).GetProperty(dto.Order) != null)// 避免sql注入
            {
                orderBy = dto.Order + (dto.Desc ? "DESC" : "ASC");
            }
            else
            {
                orderBy = "CreatedDate " + (dto.Desc ? "DESC" : "ASC");
            }

            string whereBy = null;
            if (string.IsNullOrEmpty(dto.IdOrName) == false)
            {
                whereBy = "EntityId=@IdOrName OR PlatformUserId=@IdOrName OR EOS=@IdOrName OR Name LIKE '%'||@IdOrName||'%'";
            }

            var items = await base.QueryPagedAsync(dto.PageIndex, dto.PageSize, whereBy, orderBy, dto);
            uint total = await base.QueryRecordCountAsync(whereBy, dto);

            return new PaginationResultDto() { Items = items, Total = total };
        }

        public async Task<IEnumerable<PlayerLocationDto>> QueryPlayersLocation(IEnumerable<int> filterEntitiyIds)
        {
            if (filterEntitiyIds == null)
            {
                throw new ArgumentNullException(nameof(filterEntitiyIds));
            }

            string sql = "SELECT EntityId,Name,LastPositionX,LastPositionY,LastPositionZ FROM T_Player WHERE EntityId NOT IN @FilterEntitiyIds";
            return await base.QueryAsync<PlayerLocationDto>(sql, new { FilterEntitiyIds = filterEntitiyIds });
        }
    }
}