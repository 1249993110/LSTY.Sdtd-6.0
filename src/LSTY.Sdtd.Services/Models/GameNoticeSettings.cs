using System;
using System.Collections.Generic;
using System.Text;

namespace LSTY.Sdtd.Services.Models
{
    public class GameNoticeSettings
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
