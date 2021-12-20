using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.PatronsMod.Internal
{
    class AppSettings
    {
        public string SignalRUrl { get; set; }

        public string AccessToken { get; set; }

        public bool EnableErrorPage { get; set; }

        public bool EnableCors { get; set; }
    }
}
