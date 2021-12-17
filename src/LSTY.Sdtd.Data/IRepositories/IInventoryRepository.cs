using IceCoffee.DbCore.Primitives.Repository;
using LSTY.Sdtd.Data.Entities;

namespace LSTY.Sdtd.Data.IRepositories
{
    public interface IInventoryRepository : IRepository<T_Inventory>
    {
        T_Inventory QueryBySteamId(string steamId);
    }
}
