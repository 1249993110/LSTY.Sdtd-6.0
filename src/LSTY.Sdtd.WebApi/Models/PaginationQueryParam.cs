using IceCoffee.AspNetCore.Models.RequestParams;
using NSwag.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LSTY.Sdtd.WebApi.Models
{
    /// <summary>
    /// 分页查询参数
    /// </summary>
    public class PaginationQueryParam
    {
        /// <summary>
        /// 页码
        /// </summary>
        [DefaultValue(1)]
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页数量，值为 -1 时返回所有记录
        /// </summary>
        [DefaultValue(10)]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 排序
        /// </summary>
        public string Order { get; set; }

        /// <summary>
        /// 是否降序
        /// </summary>
        public bool Desc { get; set; }
    }
}
