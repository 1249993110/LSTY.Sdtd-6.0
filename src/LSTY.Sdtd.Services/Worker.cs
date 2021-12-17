using IceCoffee.Common.Timers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Services
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly SignalRManager _signalRManager;
        private readonly FunctionManager _functionFactory;
        private readonly string _signalRUrl;

        public Worker(ILoggerFactory loggerFactory, ILogger<Worker> logger, SignalRManager signalRManager, IConfiguration configuration, FunctionManager functionFactory)
        {
            loggerFactory.CreateLogger<Worker>();
            _logger = logger;
            _signalRManager = signalRManager;
            this._functionFactory = functionFactory;
            _signalRUrl = configuration.GetSection("SignalRUrl").Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (stoppingToken.IsCancellationRequested == false)
                {
                    // 初始化功能工厂
                    _functionFactory.Initialize();

                    // 启动全局时钟
                    GlobalTimer.Start();

                    // 连接 SignalR 服务端
                    await _signalRManager.ConnectAsync(_signalRUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when Worker.ExecuteAsync running");
            }
        }
    }
}
