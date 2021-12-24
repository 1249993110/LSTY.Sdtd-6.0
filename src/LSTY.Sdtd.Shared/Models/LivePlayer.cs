using System;

namespace LSTY.Sdtd.Shared.Models
{
    public class LivePlayer : PlayerBase
    {
        public string IP { get; set; }
        public int ExpToNextLevel { get; set; }
        public int Ping { get; set; }
        public float CurrentLife { get; set; }
        public float Level { get; set; }
        public float TotalPlayTime { get; set; }
        public Position LastPosition { get; set; }
        public int Score { get; set; }
        public int ZombieKills { get; set; }

        public int PlayerKills { get; set; }

        public int Deaths { get; set; }

        public bool LandProtectionActive { get; set; }

        public float LandProtectionMultiplier { get; set; }
    }
}
