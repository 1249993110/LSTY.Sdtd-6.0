using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Models
{
    public class GameTime
    {
        /// <summary>
        /// 天
        /// </summary>
        public int Days { get; set; }

        /// <summary>
        /// 小时
        /// </summary>
        public int Hours { get; set; }

        /// <summary>
        /// 分钟
        /// </summary>
        public int Minutes { get; set; }
    }

    public class Stats
    {
        /// <summary>
        /// 游戏时间
        /// </summary>
        public GameTime GameTime { get; set; }

        /// <summary>
        /// 玩家数
        /// </summary>
        public int Players { get; set; }

        /// <summary>
        /// 僵尸数
        /// </summary>
        public int Hostiles { get; set; }

        /// <summary>
        /// 动物数
        /// </summary>
        public int Animals { get; set; }
    }
}
