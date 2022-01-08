using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Data.Dtos
{
    public class PlayerLocationDto
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
        /// X 坐标
        /// </summary>
        public int LastPositionX { get; set; }

        /// <summary>
        /// Y 坐标
        /// </summary>
        public int LastPositionY { get; set; }

        /// <summary>
        /// Z 坐标
        /// </summary>
        public int LastPositionZ { get; set; }
    }
}
