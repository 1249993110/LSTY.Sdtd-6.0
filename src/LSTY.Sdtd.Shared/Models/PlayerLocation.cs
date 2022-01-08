using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Models
{
    public class PlayerLocation
    {
        /// <summary>
        /// 实体Id
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// 实体名称，如果为空则返回或实体类名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        public Position Position { get; set; }
    }
}
