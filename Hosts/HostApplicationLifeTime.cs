using System.Threading;
using System.Threading.Tasks;
using graceful_shutdown_asp_app.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace graceful_shutdown_asp_app.Hosts
{
    public class HostApplicationLifeTime : IHostedService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ServerStateService _serverStateService;
        private readonly ILogger<HostApplicationLifeTime> _logger;

        public HostApplicationLifeTime(
            IHostApplicationLifetime hostApplicationLifetime,
            ILoggerFactory loggerFactory,
            ServerStateService serverStateService)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _serverStateService = serverStateService;
            _logger = loggerFactory.CreateLogger<HostApplicationLifeTime>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _hostApplicationLifetime.ApplicationStarted.Register(OnStarted);
            _hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
            _hostApplicationLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
        
        private void OnStarted()
        {
            _serverStateService.ApplicationStarted();
            _logger.LogInformation($"Application State: {_serverStateService.ApplicationState}");
        }

        private void OnStopping()
        {
            _serverStateService.ApplicationStopping();
            Task.Delay(12000).Wait();
            _logger.LogInformation($"Application State: {_serverStateService.ApplicationState}");
        }

        private void OnStopped()
        {
            _serverStateService.ApplicationStopped();
            _logger.LogInformation($"Application State: {_serverStateService.ApplicationState}");
        }
    }
}