using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Shared.Models
{
    /// <summary>
    /// 领地石
    /// </summary>
    public class LandClaims
    {
        /// <summary>
        /// 领地石范围
        /// </summary>
        public int Claimsize { get; set; }

        /// <summary>
        /// 领地石拥有者们
        /// </summary>
        public IEnumerable<ClaimOwner> ClaimOwners { get; set; }
    }
}
