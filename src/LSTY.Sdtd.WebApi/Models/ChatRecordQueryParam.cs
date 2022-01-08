using IceCoffee.AspNetCore.Models.RequestParams;
using IceCoffee.DataAnnotations;
using NSwag.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LSTY.Sdtd.WebApi.Models
{
    public class ChatRecordQueryParam : PaginationQueryParam
    {
        /// <summary>
        /// 指定玩家的实体Id/平台用户Id/EOS/玩家昵称
        /// </summary>
        public string IdOrName { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndDateTime { get; set; }
    }
}
