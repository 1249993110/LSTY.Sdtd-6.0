using IceCoffee.DbCore.Primitives.Repository;
using LSTY.Sdtd.Data.Entities;
using LSTY.Sdtd.Data.IRepositories;
using LSTY.Sdtd.Data;

namespace LSTY.Sdtd.Data.Repositories
{
    public class ChatRecordRepository : SQLiteRepository<T_ChatRecord>, IChatRecordRepository
    {
        public ChatRecordRepository(DefaultDbConnectionInfo dbConnectionInfo) : base(dbConnectionInfo)
        {
        }

    }
}