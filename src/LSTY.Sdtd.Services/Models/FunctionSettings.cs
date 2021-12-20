using System.ComponentModel;
using System.Xml.Serialization;

namespace LSTY.Sdtd.Services.Models
{
    public class FunctionSettings
    {
        public string SignalRUrl { get; set; }

        public string SignalRAccessToken { get; set; }

        public string ServerName { get; set; }

        public string ChatCommandPrefix { get; set; }

        public string HandleChatMessageErrorTips { get; set; }

        public GameNoticeSettings GameNoticeSettings { get; set; }
    }
}