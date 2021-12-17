using IceCoffee.DbCore.OptionalAttributes;
using IceCoffee.DbCore.Primitives.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Data.Entities
{
    public class T_Config : IEntity
    {
        [PrimaryKey, IgnoreUpdate, IgnoreInsert]
        public int Id { get; set; }

        [IgnoreUpdate, IgnoreInsert]
        public DateTime CreatedDate { get; set; }

        public DateTime LastSaveDate { get; set; }

        public string Name { get; set; }

        public string SerializedContent { get; set; }
    }
}
