using IceCoffee.DbCore.OptionalAttributes;
using IceCoffee.DbCore.Primitives.Entity;
using System;

namespace LSTY.Sdtd.Data.Entities
{
    public class T_Player : IEntity
    {
        [PrimaryKey]
        public int EntityId { get; set; }

        [IgnoreUpdate, IgnoreInsert]
        public DateTime CreatedDate { get; set; }

        public string PlatformUserId { get; set; }

        public string PlatformType { get; set; }


        public string Name { get; set; }

        public string IP { get; set; }

        /// <summary>
        /// Unit: minute
        /// </summary>
        public float TotalPlayTime { get; set; }

        public DateTime LastOnline { get; set; }

        public int LastPositionX { get; set; }

        public int LastPositionY { get; set; }

        public int LastPositionZ { get; set; }

        [Column("[Level]")]
        public float Level { get; set; }

        public int Health { get; set; }

        public float Stamina { get; set; }

        public int Score { get; set; }

        public int ZombieKills { get; set; }

        public int PlayerKills { get; set; }

        public int Deaths { get; set; }

        public string EOS { get; set; }

    }
}
