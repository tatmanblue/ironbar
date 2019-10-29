using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace noderpc.Services
{
    public class ClientNodeService : IHostedService, IDisposable
    {
        private int _executionCount = 0;
        private readonly ILogger<ClientNodeService> _logger;
        private Timer _timer;

        public ClientNodeService(ILogger<ClientNodeService> logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ClientNodeService running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ClientNodeService is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _executionCount++;

            _logger.LogInformation($"ClientNodeService is working. Count: {_executionCount}");
        }
    }
}
