using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Models
{
    public class AllowedCommandEntry
    {
        /// <summary>
        /// 命令
        /// </summary>
        public IEnumerable<string> Commands { get; set; }

        /// <summary>
        /// 权限等级
        /// </summary>
        public int PermissionLevel { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 帮助
        /// </summary>
        public string Help { get; set; }

    }
}
