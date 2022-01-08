using IceCoffee.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Models
{
    public class SpawnEntityEntry
    {
        /// <summary>
        /// 目标玩家的Id或昵称
        /// </summary>
        [Required(ErrorMessage = Resource.RequiredAttribute_ValidationError)]
        public string TargetPlayerIdOrName { get; set; }

        /// <summary>
        /// 生成的实体Id或名称
        /// </summary>
        [Required(ErrorMessage = Resource.RequiredAttribute_ValidationError)]
        public string SpawnEntityIdOrName { get; set; }
    }
}
