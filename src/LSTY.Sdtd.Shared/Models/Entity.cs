using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Models
{
    public class Entity
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public Position Position { get; set; }
        public bool IsPlayer { get; set; }
    }
}
