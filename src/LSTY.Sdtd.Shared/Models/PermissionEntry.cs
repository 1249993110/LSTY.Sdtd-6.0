using IceCoffee.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Models
{
    public class PermissionEntry
    {
        /// <summary>
        /// 命令
        /// </summary>
        [Required(ErrorMessage = Resource.RequiredAttribute_ValidationError)]
        public string Command { get; set; }

        /// <summary>
        /// 权限等级
        /// </summary>
        [Required(ErrorMessage = Resource.RequiredAttribute_ValidationError)]
        public int Level { get; set; }
    }
}
