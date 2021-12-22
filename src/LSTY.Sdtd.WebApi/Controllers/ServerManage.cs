using IceCoffee.AspNetCore;
using IceCoffee.AspNetCore.Controllers;
using IceCoffee.AspNetCore.Models.Primitives;
using LSTY.Sdtd.Services;
using LSTY.Sdtd.Services.HubReceivers;
using LSTY.Sdtd.Services.Managers;
using LSTY.Sdtd.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.WebSockets;

namespace LSTY.Sdtd.WebApi.Controllers
{
    /// <summary>
    /// ServerManage
    /// </summary>
    [Route("[controller]/[action]")]
    public class ServerManageController : ApiControllerBase
    {
        private readonly ILogger<ServerManageController> _logger;
        private readonly ILivePlayers _livePlayers;
        private readonly ServerManageHubReceiver _serverManageHub;

        public ServerManageController(ILogger<ServerManageController> logger, ILivePlayers livePlayers, SignalRManager signalRManager)
        {
            _logger = logger;
            _livePlayers = livePlayers;
            _serverManageHub = signalRManager.ServerManageHub;
        }

        /// <summary>
        /// 获取所有在线玩家
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(IEnumerable<LivePlayer>))]
        public async Task<IResponse> LivePlayers(bool realTime)
        {
            IEnumerable<LivePlayer> data = null;
            if (realTime)
            {
                data = await _serverManageHub.GetLivePlayers();
            }
            else
            {
                data = _livePlayers.Values;
            }

            return SucceededResult(data);
        }

        /// <summary>
        /// 获取指定在线玩家
        /// </summary>
        /// <returns></returns>
        [HttpGet("{entityId}")]
        [SucceededResponseType(typeof(LivePlayer))]
        public IResponse LivePlayers(int entityId)
        {
            if(_livePlayers.TryGetPlayer(entityId, out var player))
            {
                return SucceededResult(player);
            }

            return FailedResult($"Player id {entityId} not found");
        }

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(LivePlayer))]
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