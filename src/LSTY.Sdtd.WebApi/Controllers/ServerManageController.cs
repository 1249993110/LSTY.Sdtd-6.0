using IceCoffee.AspNetCore;
using IceCoffee.AspNetCore.Controllers;
using IceCoffee.AspNetCore.Models.Primitives;
using IceCoffee.DataAnnotations;
using LSTY.Sdtd.Data.IRepositories;
using LSTY.Sdtd.Services.HubReceivers;
using LSTY.Sdtd.Services.Managers;
using LSTY.Sdtd.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LSTY.Sdtd.WebApi.Controllers
{
    /// <summary>
    /// 游戏服务端管理
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
        /// <returns>命令执行结果</returns>
        [HttpGet]
        [SucceededResponseType(typeof(IEnumerable<string>))]
        public async Task<IResponse> ExecuteConsoleCommand(
            [Required(ErrorMessage = Resource.RequiredAttribute_ValidationError)] string command,
            bool inMainThread = false)
        {
            var data = await _serverManageHub.ExecuteConsoleCommand(command, inMainThread);
            return SucceededResult(data);
        }

        /// <summary>
        /// 添加管理员
        /// </summary>
        /// <param name="admins"></param>
        /// <returns>命令执行结果</returns>
        [HttpPost]
        [SucceededResponseType(typeof(IEnumerable<string>))]
        public async Task<IResponse> Admins([FromBody] IEnumerable<AdminEntry> admins)
        {
            var data = await _serverManageHub.AddAdmins(admins);
            return SucceededResult(data);
        }

        /// <summary>
        /// 删除管理员
        /// </summary>
        /// <param name="userIdentifiers"></param>
        /// <returns>命令执行结果</returns>
        [HttpDelete]
        [SucceededResponseType(typeof(IEnumerable<string>))]
        public async Task<IResponse> Admins([FromBody] IEnumerable<string> userIdentifiers)
        {
            var data = await _serverManageHub.RemoveAdmins(userIdentifiers);
            return SucceededResult(data);
        }

        /// <summary>
        /// 获取管理员
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(IEnumerable<AdminEntry>))]
        public async Task<IResponse> Admins()
        {
            var data = await _serverManageHub.GetAdmins();
            return SucceededResult(data);
        }

        /// <summary>
        /// 添加权限
        /// </summary>
        /// <param name="permissions"></param>
        /// <returns>命令执行结果</returns>
        [HttpPost]
        [SucceededResponseType(typeof(IEnumerable<string>))]
        public async Task<IResponse> Permissions(
            [FromBody][Required(ErrorMessage = Resource.RequiredAttribute_ValidationError)]
            IEnumerable<PermissionEntry> permissions)
        {
            var data = await _serverManageHub.AddPermissions(permissions);
            return SucceededResult(data);
        }

        /// <summary>
        /// 删除权限
        /// </summary>
        /// <param name="command"></param>
        /// <returns>命令执行结果</returns>
        [HttpDelete]
        [SucceededResponseType(typeof(IEnumerable<string>))]
        public async Task<IResponse> Permissions(
            [FromBody][Required(ErrorMessage = Resource.RequiredAttribute_ValidationError)]
            IEnumerable<string> command)
        {
            var data = await _serverManageHub.RemovePermissions(command);
            return SucceededResult(data);
        }

        /// <summary>
        /// 获取权限
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(IEnumerable<PermissionEntry>))]
        public async Task<IResponse> Permissions()
        {
            var data = await _serverManageHub.GetPermissions();
            return SucceededResult(data);
        }

        /// <summary>
        /// 添加白名单
        /// </summary>
        /// <param name="whitelist"></param>
        /// <returns>命令执行结果</returns>
        [HttpPost]
        [SucceededResponseType(typeof(IEnumerable<string>))]
        public async Task<IResponse> Whitelist(
            [FromBody][Required(ErrorMessage = Resource.RequiredAttribute_ValidationError)]
            IEnumerable<WhitelistEntry> whitelist)
        {
            var data = await _serverManageHub.AddWhitelist(whitelist);
            return SucceededResult(data);
        }

        /// <summary>
        /// 删除白名单
        /// </summary>
        /// <param name="userIdentifiers"></param>
        /// <returns>命令执行结果</returns>
        [HttpDelete]
        [SucceededResponseType(typeof(IEnumerable<string>))]
        public async Task<IResponse> Whitelist(
            [FromBody][Required(ErrorMessage = Resource.RequiredAttribute_ValidationError)]
            IEnumerable<string> userIdentifiers)
        {
            var data = await _serverManageHub.RemoveWhitelist(userIdentifiers);
            return SucceededResult(data);
        }

        /// <summary>
        /// 获取白名单
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(IEnumerable<WhitelistEntry>))]
        public async Task<IResponse> Whitelist()
        {
            var data = await _serverManageHub.GetWhitelist();
            return SucceededResult(data);
        }

        /// <summary>
        /// 添加黑名单
        /// </summary>
        /// <param name="blacklist"></param>
        /// <returns>命令执行结果</returns>
        [HttpPost]
        [SucceededResponseType(typeof(IEnumerable<string>))]
        public async Task<IResponse> Blacklist(
            [FromBody][Required(ErrorMessage = Resource.RequiredAttribute_ValidationError)]
            IEnumerable<BlacklistEntry> blacklist)
        {
            var data = await _serverManageHub.AddBlacklist(blacklist);
            return SucceededResult(data);
        }

        /// <summary>
        /// 删除黑名单
        /// </summary>
        /// <param name="userIdentifiers"></param>
        /// <returns>命令执行结果</returns>
        [HttpDelete]
        [SucceededResponseType(typeof(IEnumerable<string>))]
        public async Task<IResponse> Blacklist([FromBody] IEnumerable<string> userIdentifiers)
        {
            var data = await _serverManageHub.RemoveBlacklist(userIdentifiers);
            return SucceededResult(data);
        }

        /// <summary>
        /// 获取黑名单
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(IEnumerable<BlacklistEntry>))]
        public async Task<IResponse> Blacklist()
        {
            var data = await _serverManageHub.GetBlacklist();
            return SucceededResult(data);
        }

        /// <summary>
        /// 发送全局消息
        /// </summary>
        /// <param name="globalMessage"></param>
        /// <returns>命令执行结果</returns>
        [HttpPost]
        [SucceededResponseType(typeof(IEnumerable<string>))]
        public async Task<IResponse> SendGlobalMessage([FromBody] GlobalMessage globalMessage)
        {
            var data = await _serverManageHub.SendGlobalMessage(globalMessage);
            return SucceededResult(data);
        }

        /// <summary>
        /// 发送私聊消息
        /// </summary>
        /// <param name="privateMessage"></param>
        /// <returns>命令执行结果</returns>
        [HttpPost]
        [SucceededResponseType(typeof(IEnumerable<string>))]
        public async Task<IResponse> SendPrivateMessage([FromBody] PrivateMessage privateMessage)
        {
            var data = await _serverManageHub.SendPrivateMessage(privateMessage);
            return SucceededResult(data);
        }

        /// <summary>
        /// 传送玩家
        /// </summary>
        /// <param name="teleportEntry"></param>
        /// <returns>命令执行结果</returns>
        [HttpPost]
        [SucceededResponseType(typeof(IEnumerable<string>))]
        public async Task<IResponse> TeleportPlayer(TeleportEntry teleportEntry)
        {
            var data = await _serverManageHub.TeleportPlayer(teleportEntry);
            return SucceededResult(data);
        }

        /// <summary>
        /// 给予物品
        /// </summary>
        /// <param name="giveItemEntry"></param>
        /// <returns>命令执行结果</returns>
        [HttpPost]
        [SucceededResponseType(typeof(IEnumerable<string>))]
        public async Task<IResponse> GiveItem(GiveItemEntry giveItemEntry)
        {
            var data = await _serverManageHub.GiveItem(giveItemEntry);
            return SucceededResult(data);
        }

        /// <summary>
        /// 生成实体
        /// </summary>
        /// <param name="spawnEntityEntry"></param>
        /// <returns>命令执行结果</returns>
        [HttpPost]
        [SucceededResponseType(typeof(IEnumerable<string>))]
        public async Task<IResponse> SpawnEntity(SpawnEntityEntry spawnEntityEntry)
        {
            var data = await _serverManageHub.SpawnEntity(spawnEntityEntry);
            return SucceededResult(data);
        }

        /// <summary>
        /// 获取允许生成的实体列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(IEnumerable<AllowSpawnedEntity>))]
        public async Task<IResponse> AllowSpawnedEntities()
        {
            var data = await _serverManageHub.GetAllowSpawnedEntities();
            return SucceededResult(data);
        }

        /// <summary>
        /// 获取允许执行的命令列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(IEnumerable<AllowedCommandEntry>))]
        public async Task<IResponse> AllowedCommands()
        {
            var data = await _serverManageHub.GetAllowedCommands();
            return SucceededResult(data);
        }

        /// <summary>
        /// 获取统计数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(Stats))]
        public async Task<IResponse> Stats()
        {
            var data = await _serverManageHub.GetStats();
            return SucceededResult(data);
        }

        /// <summary>
        /// 重启游戏服务端
        /// </summary>
        /// <param name="force">是否强制重启</param>
        /// <returns>命令执行结果</returns>
        [HttpPost]
        [SucceededResponseType(typeof(IEnumerable<string>))]
        public async Task<IResponse> RestartServer(bool force = false)
        {
            var data = await _serverManageHub.RestartServer();
            return SucceededResult(data);
        }

        /// <summary>
        /// 获取动物位置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(IEnumerable<EntityLocation>))]
        public async Task<IResponse> AnimalsLocation()
        {
            var data = await _serverManageHub.GetAnimalsLocation();
            return SucceededResult(data);
        }

        /// <summary>
        /// 获取僵尸位置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(IEnumerable<EntityLocation>))]
        public async Task<IResponse> HostileLocation()
        {
            var data = await _serverManageHub.GetHostileLocation();
            return SucceededResult(data);
        }

        /// <summary>
        /// 获取玩家位置
        /// </summary>
        /// <param name="offline">是否包含离线玩家</param>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(IEnumerable<Models.PlayerLocation>))]
        public async Task<IResponse> PlayersLocation(bool offline = false)
        {
            var data = new List<Models.PlayerLocation>();

            var livePlayersLocation = await _serverManageHub.GetPlayersLocation();

            if (offline)
            {
                var playerRepository = HttpContext.RequestServices.GetRequiredService<IPlayerRepository>();
                var historyPlayersLocation = await playerRepository.QueryPlayersLocation(livePlayersLocation.Select(s => s.EntityId));

                foreach (var item in historyPlayersLocation)
                {
                    data.Add(new Models.PlayerLocation() 
                    {
                        EntityId = item.EntityId,
                        Name = item.Name,
                        Position = new Position()
                        {
                            X = item.LastPositionX,
                            Y = item.LastPositionY,
                            Z = item.LastPositionZ
                        }
                    });
                }

                foreach (var item in livePlayersLocation)
                {
                    data.Add(new Models.PlayerLocation()
                    {
                        EntityId = item.EntityId,
                        Name = item.Name,
                        Position = item.Position,
                        IsOnline = true
                    });
                }

                return SucceededResult(data);
            }
            else
            {
                foreach (var item in livePlayersLocation)
                {
                    data.Add(new Models.PlayerLocation()
                    {
                        EntityId = item.EntityId,
                        Name = item.Name,
                        Position = item.Position,
                        IsOnline = true
                    });
                }

                return SucceededResult(data);
            }
        }

        /// <summary>
        /// 获取领地石
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(LandClaims))]
        public async Task<IResponse> LandClaims()
        {
            var data = await _serverManageHub.GetLandClaims();
            return SucceededResult(data);
        }

        /// <summary>
        /// 获取指定玩家的领地石
        /// </summary>
        /// <param name="entityId">玩家的实体Id</param>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(ClaimOwner))]
        public async Task<IResponse> LandClaim(int entityId)
        {
            var data = await _serverManageHub.GetLandClaim(entityId);
            return SucceededResult(data);
        }

        /// <summary>
        /// 获取物品和方块
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSzie">每页数量，值为 -1 时返回所有记录</param>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(ItemBlockPaged))]
        public async Task<IResponse> ItemBlocks(int pageIndex = 1, int pageSzie = 10)
        {
            var data = await _serverManageHub.GetItemBlocks(pageIndex, pageSzie);
            return SucceededResult(data);
        }

        /// <summary>
        /// 获取本地化字典
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(IDictionary<string, string>))]
        public async Task<IResponse> Localization(string language = "schinese")
        {
            var data = await _serverManageHub.GetLocalization(language);
            return SucceededResult(data);
        }

        /// <summary>
        /// 获取本地化字符串
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        [HttpGet("{itemName}")]
        [SucceededResponseType(typeof(string))]
        public async Task<IResponse> Localization(
            [FromRoute][Required(ErrorMessage = Resource.RequiredAttribute_ValidationError)]string itemName, 
            string language = "schinese")
        {
            var data = await _serverManageHub.GetLocalization(itemName, language);
            return SucceededResult(data);
        }

        /// <summary>
        /// 获取已知的语言列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SucceededResponseType(typeof(IEnumerable<string>))]
        public async Task<IResponse> KnownLanguages()
        {
            var data = await _serverManageHub.GetKnownLanguages();
            return SucceededResult(data);
        }
    }
}