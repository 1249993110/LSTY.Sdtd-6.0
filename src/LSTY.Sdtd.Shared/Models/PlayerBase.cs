using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Models
{
    public class PlayerBase
    {
        public string PlatformUserId { get; set; }
        public string PlatformType { get; set; }
        public int EntityId { get; set; }
        public string Name { get; set; }
        public string EOS { get; set; }
    }
}
