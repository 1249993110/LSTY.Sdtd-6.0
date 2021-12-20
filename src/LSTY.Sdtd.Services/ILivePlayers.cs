using LSTY.Sdtd.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LSTY.Sdtd.Services
{
    public interface ILivePlayers
    {
        LivePlayer this[int entityId] { get; }

        /// <summary>
        /// 隐藏默认的 TryGetValue，如果本地不存在玩家，则通过 SignalR 从游戏服务器获取
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="livePlayer"></param>
        /// <returns></returns>
        bool TryGetPlayer(int entityId, out LivePlayer livePlayer);

        IEnumerable<int> Keys { get; }

        IEnumerable<LivePlayer> Values { get; }

        bool IsEmpty { get; }

        int Count { get; }

        event Action ServerNonePlayer;

        event Action ServerHavePlayerAgain;
    }
}
