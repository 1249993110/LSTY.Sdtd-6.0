using LSTY.Sdtd.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LSTY.Sdtd.WebApi.Models
{
    public class AppSettings
    {
        public string[] Cors { get; set; }

        public string Urls { get; set; }

        public string SignalRUrl { get; set; }

        public FunctionConfigs FunctionConfigs { get; set; }
    }
}
