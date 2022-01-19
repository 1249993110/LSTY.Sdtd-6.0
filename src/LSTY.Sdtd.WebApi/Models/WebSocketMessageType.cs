using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.WebApi.Models
{
    public struct WebSocketMessageTypes
    {
        public const string ConsoleLog = nameof(ConsoleLog);
        public const string PlayerUpdate = nameof(PlayerUpdate);
        public const string ChatMessage = nameof(ChatMessage);
    }
}
