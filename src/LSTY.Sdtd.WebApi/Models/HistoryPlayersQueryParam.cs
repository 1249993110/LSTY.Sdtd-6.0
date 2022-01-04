using IceCoffee.AspNetCore.Models.RequestParams;
using NSwag.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LSTY.Sdtd.WebApi.Models
{
    public class HistoryPlayersQueryParam : PaginationQueryParam
    {
        /// <summary>
        /// 指定玩家的实体Id/平台用户Id/EOS/玩家昵称
        /// </summary>
        public string IdOrName { get; set; }
    }
}
