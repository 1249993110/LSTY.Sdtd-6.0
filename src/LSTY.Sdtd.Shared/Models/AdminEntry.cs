using IceCoffee.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Models
{
    public class AdminEntry
    {
        /// <summary>
        /// 平台类型 + 平台用户Id，如 EOS_XXXX 或 Steam_XXXX
        /// </summary>
        [Required(ErrorMessage = Resource.RequiredAttribute_ValidationError)]
        public string UserIdentifier { get; set; }

        /// <summary>
        /// 权限等级
        /// </summary>
        [Required(ErrorMessage = Resource.RequiredAttribute_ValidationError)]
        public int PermissionLevel { get; set; }

        /// <summary>
        /// 显示名称，默认为玩家昵称
        /// </summary>
        public string DisplayName { get; set; }
    }
}
