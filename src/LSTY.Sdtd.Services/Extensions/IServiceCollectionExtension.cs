using LSTY.Sdtd.Services.Managers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Services.Extensions
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddSdtd(this IServiceCollection services)
        {
            services.AddSingleton<SignalRManager>();
            services.AddSingleton<FunctionFactory>();
            services.AddSingleton<ILivePlayers, LivePlayers>();
            services.AddSingleton<PersistentManager>();
            services.AddHostedService<Worker>();

            return services;
        }
    }
}
