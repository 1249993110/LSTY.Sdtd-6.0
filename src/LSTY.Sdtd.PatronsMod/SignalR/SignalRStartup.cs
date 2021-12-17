using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Owin;

namespace LSTY.Sdtd.PatronsMod.SignalR
{
    public class SignalRStartup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR(new HubConfiguration()
            {
                EnableJSONP = false,
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = false
            });

            if (string.IsNullOrEmpty(ModApi.AppSettings.AccessToken) == false)
            {
                var globalAuthorizer = new QueryStringAuthorizeAttribute();
                GlobalHost.HubPipeline.AddModule(new AuthorizeModule(globalAuthorizer, null));
            }
        }
    }
}