using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Models
{
    public class WebSocketMessage
    {
        public WebSocketMessageType MessageType { get; set; }

        public object MessageEntity { get; set; }
    }
}
