using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Models
{
    public class ChatMessage
    {
        public int EntityId { get; set; }

        public string SenderName { get; set; }

        public ChatType ChatType { get; set; }

        public string Message { get; set; }
    }
}
