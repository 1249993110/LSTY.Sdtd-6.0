using System.Collections.Generic;

namespace LSTY.Sdtd.Shared.Models
{
    public class Inventory
    {
        public List<InvItem> Bag { get; set; }
        public List<InvItem> Belt { get; set; }
        public InvItem[] Equipment { get; set; }
    }
}
