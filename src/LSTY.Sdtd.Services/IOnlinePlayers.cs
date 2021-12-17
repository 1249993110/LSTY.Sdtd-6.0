using LSTY.Sdtd.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LSTY.Sdtd.Services
{
    public interface IOnlinePlayers
    {
        OnlinePlayer this[string steamId] { get; }

        /// <summary>
        /// 隐藏默认的 TryGetValue，如果本地不存在玩家，则通过 SignalR 从游戏服务器获取
        /// </summary>
        /// <param name="steamId"></param>
        /// <param name="onlinePlayer"></param>
        /// <returns></returns>
        bool TryGetPlayer(string steamId, out OnlinePlayer onlinePlayer);

        IEnumerable<string> Keys { get; }

        IEnumerable<OnlinePlayer> Values { get; }

        bool IsEmpty { get; }

        int Count { get; }

        event Action ServerNonePlayer;

        event Action ServerHavePlayerAgain;
    }
}
