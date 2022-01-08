using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Models
{
    public class ClaimOwner
    {
        /// <summary>
        /// 是否激活
        /// </summary>
        public bool ClaimActive { get; set; }

        public int EntityId { get; set; }

        public IEnumerable<Position> Claims { get; set; }
    }
}
