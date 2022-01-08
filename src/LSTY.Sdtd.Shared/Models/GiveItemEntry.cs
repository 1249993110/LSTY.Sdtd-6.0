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
    public class GiveItemEntry
    {
        /// <summary>
        /// 目标玩家的Id或昵称
        /// </summary>
        [Required(ErrorMessage = Resource.RequiredAttribute_ValidationError)]
        public string TargetPlayerIdOrName { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        [Required(ErrorMessage = Resource.RequiredAttribute_ValidationError)]
        public string ItemName { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        [DefaultValue(1), Range(1, 999999, ErrorMessage = Resource.RangeAttribute_ValidationError)]
        public int? Count { get; set; }

        /// <summary>
        /// 质量
        /// </summary>
        [DefaultValue(1), Range(1, 6, ErrorMessage = Resource.RangeAttribute_ValidationError)]
        public int? Quality { get; set; }

        /// <summary>
        /// 耐久度百分比
        /// </summary>
        [DefaultValue(100), Range(1, 100, ErrorMessage = Resource.RangeAttribute_ValidationError)]
        public int? Durability { get; set; }
    }
}
