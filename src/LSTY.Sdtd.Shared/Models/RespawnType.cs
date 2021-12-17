using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Models
{
    public enum RespawnType : byte
    {
        NewGame = 0,
        LoadedGame = 1,
        Died = 2,
        Teleport = 3,
        EnterMultiplayer = 4,
        JoinMultiplayer = 5,
        Unknown = 6
    }
}
