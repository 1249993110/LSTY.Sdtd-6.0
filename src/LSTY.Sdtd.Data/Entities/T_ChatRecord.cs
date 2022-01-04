using IceCoffee.DbCore.OptionalAttributes;
using IceCoffee.DbCore.Primitives.Entity;
using System;

namespace LSTY.Sdtd.Data.Entities
{
    public class T_ChatRecord : IEntity
    {
        [PrimaryKey, IgnoreUpdate, IgnoreInsert]
        public int Id { get; set; }

        [IgnoreUpdate, IgnoreInsert]
        public DateTime CreatedDate { get; set; }

        public int EntityId { get; set; }

        public string SenderName { get; set; }

        public int ChatType { get; set; }

        public string Message { get; set; }

    }
}