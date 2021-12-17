namespace LSTY.Sdtd.Services.Configs
{
    public class FunctionConfig
    {
        public string ServerName { get; set; }

        public string ChatCommandPrefix { get; set; }

        public string HandleChatMessageErrorTips { get; set; }

        public GameNoticeConfig GameNoticeConfig { get; set; }
    }
}