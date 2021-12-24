using IceCoffee.DbCore.ExceptionCatch;
using IceCoffee.DbCore.Primitives.Repository;
using LSTY.Sdtd.Data.Entities;
using LSTY.Sdtd.Data.IRepositories;
using System.Linq;

namespace LSTY.Sdtd.Data.Repositories
{
    public class InventoryRepository : SQLiteRepository<T_Inventory>, IInventoryRepository
    {
        public InventoryRepository(DefaultDbConnectionInfo dbConnectionInfo) : base(dbConnectionInfo)
        {
        }
    }
}
