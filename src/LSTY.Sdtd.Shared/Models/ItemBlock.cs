using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Models
{
    public class ItemBlock
    {
        public int Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// 是否为方块
        /// </summary>
        public bool IsBlock { get; set; }
    }

    public class ItemBlockPaged
    {
        public uint Total { get; set; }  

        public IEnumerable<ItemBlock> Items { get; set; } 
    }
}
