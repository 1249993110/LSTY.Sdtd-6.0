using IceCoffee.AspNetCore;
using IceCoffee.AspNetCore.Controllers;
using IceCoffee.AspNetCore.Models.Primitives;
using LSTY.Sdtd.Data.Entities;
using LSTY.Sdtd.Data.IRepositories;
using LSTY.Sdtd.Services;
using LSTY.Sdtd.Services.HubReceivers;
using LSTY.Sdtd.Services.Managers;
using LSTY.Sdtd.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;

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
            SignalRManager signalRManager)
        {
            _logger = logger;
            _serverManageHub = signalRManager.ServerManageHub;
        }

        /// <summary>
        /// 执行控制台命令
        /// </summary>
        /// <param name="command">欲执行的命令</param>
        /// <param name="inMainThread">是否在主线程执行此命令，注意此操作可能造成游戏服务端卡顿</param>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(List<string>))]
        public async Task<IResponse> ExecuteConsoleCommand(string command, bool inMainThread = false)
        {
            if (string.IsNullOrEmpty(command))
            {
                return FailedResult("Command can not be empty");
            }

            var data = await _serverManageHub.ExecuteConsoleCommand(command, inMainThread);
            return SucceededResult(data);
        }
    }
}