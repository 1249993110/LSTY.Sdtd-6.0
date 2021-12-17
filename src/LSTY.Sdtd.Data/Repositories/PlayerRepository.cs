using IceCoffee.DbCore.Primitives.Repository;
using LSTY.Sdtd.Data.Entities;
using LSTY.Sdtd.Data.IRepositories;

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
                return base.Query("Name LIKE CONCAT('%',@Keyword,'%')", "CreatedDate DESC", new { Keyword = playerName });
            }
        }

        public IEnumerable<T_Player> QueryByEntityId(int entityId, bool fuzzySearch = false)
        {
            if (fuzzySearch == false)
            {
                return base.QueryById(nameof(T_Player.EntityId), entityId);
            }
            else
            {
                return base.Query("EntityId LIKE CONCAT('%',@Keyword,'%')", "CreatedDate DESC", new { Keyword = entityId });
            }
        }

        public IEnumerable<T_Player> QueryByPlatformUserId(string platformUserId, bool fuzzySearch = false)
        {
            if (fuzzySearch == false)
            {
                return base.QueryById(nameof(T_Player.PlatformUserId), platformUserId);
            }
            else
            {
                return base.Query("PlatformUserId LIKE CONCAT('%',@Keyword,'%')", "CreatedDate DESC", new { Keyword = platformUserId });
            }
        }
    }
}