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
    public class GlobalMessage
    {
        /// <summary>
        /// 消息
        /// </summary>
        [Required(ErrorMessage = Resource.RequiredAttribute_ValidationError)]
        public string Message { get; set; }

        /// <summary>
        /// 发送者昵称
        /// </summary>
        [DefaultValue(ExportedConstants.DefaultServerName)]
        public string SenderName { get; set; }
    }
}
