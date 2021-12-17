using System;
using System.Collections.Generic;
using System.Text;

namespace LSTY.Sdtd.Services.Configs
{
    public class GameNoticeConfig
    {
        /// <summary>
        /// 是否启用功能
        /// </summary>
        public bool IsEnabled { get; set; }

        public string WelcomeNotice { get; set; }

        public int AlternateInterval { get; set; }

        public string[] AlternateNotices { get; set; }
    }
}
