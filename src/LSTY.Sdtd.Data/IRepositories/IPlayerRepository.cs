using IceCoffee.DbCore.Primitives.Repository;
using LSTY.Sdtd.Data.Dtos;
using LSTY.Sdtd.Data.Entities;
using System.Collections.Generic;

namespace LSTY.Sdtd.Data.IRepositories
{
    public interface IPlayerRepository : IRepository<T_Player>
    {
        IEnumerable<T_Player> QueryByPlayerName(string playerName, bool fuzzySearch = false);

        IEnumerable<T_Player> QueryByEntityId(int entityId);

        IEnumerable<T_Player> QueryByPlatformUserId(string platformUserId);

        IEnumerable<T_Player> QueryByEOS(string EOS);

        Task<PaginationResultDto> QueryPagedAsync(PlayersQueryDto dto);
    }
}
