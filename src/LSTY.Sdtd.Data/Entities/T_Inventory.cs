using IceCoffee.DbCore.OptionalAttributes;
using IceCoffee.DbCore.Primitives.Entity;
using System;

namespace LSTY.Sdtd.Data.Entities
{
    public class T_Inventory : IEntity
    {
        [PrimaryKey]
        public int EntityId { get; set; }

        [IgnoreUpdate, IgnoreInsert]
        public DateTime CreatedDate { get; set; }

        public string SerializedContent { get; set; }
    }
}
