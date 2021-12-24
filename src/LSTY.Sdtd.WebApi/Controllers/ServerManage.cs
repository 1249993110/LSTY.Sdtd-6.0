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
        private readonly ILivePlayers _livePlayers;
        private readonly ServerManageHubReceiver _serverManageHub;
        private readonly IInventoryRepository _inventoryRepository;

        public ServerManageController(ILogger<ServerManageController> logger,
            ILivePlayers livePlayers,
            SignalRManager signalRManager, 
            IInventoryRepository inventoryRepository)
        {
            _logger = logger;
            _livePlayers = livePlayers;
            _serverManageHub = signalRManager.ServerManageHub;
            _inventoryRepository=inventoryRepository;
        }

        /// <summary>
        /// 执行控制台命令
        /// </summary>
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

            return FailedResult($"Player {entityId} not found");
        }

        /// <summary>
        /// 获取玩家背包
        /// </summary>
        /// <returns></returns>
        [HttpGet("{entityId}")]
        [SucceededResponseType(typeof(Inventory))]
        public async Task<IResponse> PlayerInventory(int entityId)
        {
            Inventory data = null;
            if (_livePlayers.ContainsPlayer(entityId))
            {
                data = await _serverManageHub.GetLivePlayerInventory(entityId);

                return SucceededResult(data);
            }

            var entities = await _inventoryRepository.QueryByIdAsync(nameof(T_Inventory.EntityId), entityId);
            if(entities?.Any() == false)
            {
                return FailedResult($"Player {entityId} not found");
            }

            data = JsonSerializer.Deserialize<Inventory>(entities.First().SerializedContent);

            return SucceededResult(data);
        }
    }
}