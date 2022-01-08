using IceCoffee.AspNetCore;
using IceCoffee.AspNetCore.Controllers;
using IceCoffee.AspNetCore.Models.Primitives;
using IceCoffee.AspNetCore.Models.ResponseResults;
using LSTY.Sdtd.Data.Dtos;
using LSTY.Sdtd.Data.Entities;
using LSTY.Sdtd.Data.IRepositories;
using LSTY.Sdtd.Services;
using LSTY.Sdtd.Services.HubReceivers;
using LSTY.Sdtd.Services.Managers;
using LSTY.Sdtd.Shared.Models;
using LSTY.Sdtd.WebApi.Models;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LSTY.Sdtd.WebApi.Controllers
{
    /// <summary>
    /// 玩家
    /// </summary>
    [Route("[action]")]
    public class PlayersController : ApiControllerBase
    {
        private readonly ILogger<PlayersController> _logger;
        private readonly ILivePlayers _livePlayers;
        private readonly ServerManageHubReceiver _serverManageHub;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IPlayerRepository _playerRepository;

        public PlayersController(ILogger<PlayersController> logger,
            ILivePlayers livePlayers,
            SignalRManager signalRManager,
            IInventoryRepository inventoryRepository,
            IPlayerRepository playerRepository)
        {
            _logger = logger;
            _livePlayers = livePlayers;
            _serverManageHub = signalRManager.ServerManageHub;
            _inventoryRepository = inventoryRepository;
            _playerRepository = playerRepository;
        }

        /// <summary>
        /// 获取所有在线玩家
        /// </summary>
        /// <param name="realTime">是否实时从游戏服务端获取</param>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(IEnumerable<LivePlayer>))]
        public async Task<IResponse> LivePlayers(bool realTime)
        {
            IEnumerable<LivePlayer> data = null;
            if (realTime)
            {
                data = await _serverManageHub.GetPlayers();
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
        /// <param name="entityId">指定玩家的实体Id</param>
        /// <param name="realTime">是否实时从游戏服务端获取</param>
        /// <returns></returns>
        [HttpGet("{entityId}")]
        [SucceededResponseType(typeof(LivePlayer))]
        public async Task<IResponse> LivePlayers(int entityId, bool realTime)
        {
            if (realTime)
            {
                var player = await _serverManageHub.GetPlayer(entityId);
                if (player == null)
                {
                    goto _Return;
                }

                return SucceededResult(player);
            }
            else
            {
                if (_livePlayers.TryGetPlayer(entityId, out var player))
                {
                    return SucceededResult(player);
                }
            }

        _Return:
            return FailedResult($"Player {entityId} not found");
        }

        /// <summary>
        /// 获取玩家背包
        /// </summary>
        /// <param name="entityId">指定玩家的实体Id</param>
        /// <returns></returns>
        [HttpGet("{entityId}")]
        [SucceededResponseType(typeof(Inventory))]
        public async Task<IResponse> PlayerInventory(int entityId)
        {
            Inventory data = null;
            if (_livePlayers.ContainsPlayer(entityId))
            {
                data = await _serverManageHub.GetPlayerInventory(entityId);

                return SucceededResult(data);
            }

            var entities = await _inventoryRepository.QueryByIdAsync(nameof(T_Inventory.EntityId), entityId);
            if (entities?.Any() == false)
            {
                return FailedResult($"Player {entityId} not found");
            }

            data = JsonSerializer.Deserialize<Inventory>(entities.First().SerializedContent, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return SucceededResult(data);
        }

        /// <summary>
        /// 获取历史玩家
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(PaginationQueryResult<T_Player>))]
        public async Task<IResponse> HistoryPlayers([FromQuery] HistoryPlayersQueryParam models)
        {
            var data = await _playerRepository.QueryPagedAsync(models.Adapt<PlayersQueryDto>());

            return PaginationQueryResult(data.Items, data.Total);
        }

        /// <summary>
        /// 获取指定历史玩家
        /// </summary>
        /// <param name="entityId">指定玩家的实体Id</param>
        /// <returns></returns>
        [HttpGet("{entityId}")]
        [SucceededResponseType(typeof(T_Player))]
        public async Task<IResponse> HistoryPlayers(int entityId)
        {
            var player = await _playerRepository.QueryByIdAsync(nameof(T_Player.EntityId), entityId);

            if(player?.Any() == false)
            {
                return FailedResult($"Player {entityId} not found");
            }

            return SucceededResult(player.First());
        }
    }
}