using IceCoffee.AspNetCore;
using IceCoffee.AspNetCore.Controllers;
using IceCoffee.AspNetCore.Models.Primitives;
using IceCoffee.AspNetCore.Models.ResponseResults;
using LSTY.Sdtd.Data.Dtos;
using LSTY.Sdtd.Data.Entities;
using LSTY.Sdtd.Data.IRepositories;
using LSTY.Sdtd.WebApi.Models;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace LSTY.Sdtd.WebApi.Controllers
{
    /// <summary>
    /// 聊天记录
    /// </summary>
    [Route("[controller]")]
    public class ChatRecordController : ApiControllerBase
    {
        private readonly ILogger<ChatRecordController> _logger;
        private readonly IVChatRecordRepository _vChatRecordRepository;

        public ChatRecordController(ILogger<ChatRecordController> logger, IVChatRecordRepository vChatRecordRepository)
        {
            _logger = logger;
            _vChatRecordRepository = vChatRecordRepository;
        }

        /// <summary>
        /// 获取聊天记录
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(PaginationQueryResult<V_ChatRecord>))]
        public async Task<IResponse> Get([FromQuery] ChatRecordQueryParam models)
        {
            var data = await _vChatRecordRepository.QueryPagedAsync(models.Adapt<ChatRecordQueryDto>());

            return PaginationQueryResult(data.Items, data.Total);
        }
    }
}