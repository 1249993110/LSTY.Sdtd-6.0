using IceCoffee.DbCore.Primitives.Repository;
using LSTY.Sdtd.Data.Entities;
using LSTY.Sdtd.Data.IRepositories;
using LSTY.Sdtd.Data;

namespace LSTY.Sdtd.Data.Repositories
{
    public class ConfigRepository : SQLiteRepository<T_Config>, IConfigRepository
    {
        public ConfigRepository(DefaultDbConnectionInfo dbConnectionInfo) : base(dbConnectionInfo)
        {
        }

    }
}