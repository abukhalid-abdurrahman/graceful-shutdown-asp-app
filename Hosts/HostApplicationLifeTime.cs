using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace graceful_shutdown_asp_app.Hosts
{
    public class HostApplicationLifeTime : IHostedService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public HostApplicationLifeTime(
            IHostApplicationLifetime hostApplicationLifetime)
            => _hostApplicationLifetime = hostApplicationLifetime;
        
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
            Console.WriteLine("Started...");
        }

        private void OnStopping()
        {
            Console.WriteLine("Stopping...");
        }

        private void OnStopped()
        {
            Console.WriteLine("Stopped...");
        }
    }
}