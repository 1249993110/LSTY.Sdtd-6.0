using IceCoffee.Common.Timers;
using IceCoffee.DbCore;
using LSTY.Sdtd.Services.Managers;
using LSTY.Sdtd.Services.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Services
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly SignalRManager _signalRManager;
        private readonly FunctionFactory _functionManager;

        public Worker(ILogger<Worker> logger, SignalRManager signalRManager, FunctionFactory functionManager)
        {
            _logger = logger;
            _signalRManager = signalRManager;
            _functionManager = functionManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (stoppingToken.IsCancellationRequested == false)
                {
                    // 连接 SignalR 服务端
                    await _signalRManager.ConnectAsync();

                    // 初始化功能
                    _functionManager.Initialize();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when Worker.ExecuteAsync running");
            }
        }
    }
}
