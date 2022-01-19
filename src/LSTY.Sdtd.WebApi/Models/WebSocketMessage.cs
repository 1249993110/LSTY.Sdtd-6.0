using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.WebApi.Models
{
    public class WebSocketMessage
    {
        public string EventType { get; set; }

        public object Payload { get; set; }
    }
}
