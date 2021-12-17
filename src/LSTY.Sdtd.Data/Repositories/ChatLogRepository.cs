using IceCoffee.DbCore.Primitives.Repository;
using LSTY.Sdtd.Data.Entities;
using LSTY.Sdtd.Data.IRepositories;
using LSTY.Sdtd.Data;

namespace LSTY.Sdtd.Data.Repositories
{
    public class ChatLogRepository : SQLiteRepository<T_ChatLog>, IChatLogRepository
    {
        public ChatLogRepository(DefaultDbConnectionInfo dbConnectionInfo) : base(dbConnectionInfo)
        {
        }

    }
}