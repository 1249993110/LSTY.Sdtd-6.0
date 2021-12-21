using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LSTY.Sdtd.WebApi.Models
{
    public class AppSettings
    {
        public string Urls { get; set; }

        public string AccessToken { get; set; }

        public bool EnableSwagger { get; set; }

        public bool EnableCors { get; set; }

    }
}
