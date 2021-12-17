using IceCoffee.AspNetCore.Controllers;
using IceCoffee.AspNetCore.Models.Primitives;
using LSTY.Sdtd.Services.HubReceivers;
using LSTY.Sdtd.WebApi.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace LSTY.Sdtd.WebApi.Controllers
{
    /// <summary>
    /// ServerManage
    /// </summary>
    [Route("[controller]/[action]")]
    public class ServerManageController : ApiControllerBase
    {
        private readonly ILogger<ServerManageController> _logger;
        private readonly ServerManageHubReceiver _serverManageHub;


        public ServerManageController(ILogger<ServerManageController> logger,
            ServerManageHubReceiver serverManageHub)
        {
            _logger = logger;
            _serverManageHub = serverManageHub;
        }


        /// <summary>
        /// 获取在线玩家
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IResponse> GetOnlinePlayers()
        {
            var data = await _serverManageHub.GetOnlinePlayers();
            return SucceededResult(data);
        }
    }
}
