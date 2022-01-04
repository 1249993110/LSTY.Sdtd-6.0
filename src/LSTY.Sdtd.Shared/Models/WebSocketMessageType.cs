using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Models
{
    public enum WebSocketMessageType : byte
    {
        ConsoleLog,

        PlayerUpdate,

        ChatMessage
    }
}
