using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Models
{
    public class PlayerSpawnedEventArgs
    {
        public int EntityId { get; set; }

        public RespawnType RespawnType { get; set; }

        public Position Position { get; set; }
    }
}
