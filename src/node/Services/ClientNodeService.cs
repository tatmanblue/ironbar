using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace node.Services
{
    public enum ClientNodeServiceState
    {
        NotStarted,
        ConnectingToBootNode,
        Running,
        Halted,
        RetryingConnectionToBootNode,
    }

    public class ClientNodeService : IHostedService, IDisposable
    {
        private readonly ILogger<ClientNodeService> logger;
        private Timer doWorkDelay;
        private ClientNodeServiceState serviceState = ClientNodeServiceState.NotStarted;

        public ClientNodeService(ILogger<ClientNodeService> logger)
        {
            this.logger = logger;
        }

        public void Dispose()
        {
            doWorkDelay?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("ClientNodeService is initializing.");

            doWorkDelay = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("ClientNodeService is stopping.");

            doWorkDelay.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            logger.LogInformation($"ClientNodeService is state {serviceState}");

            switch (serviceState)
            {
                case ClientNodeServiceState.NotStarted:
                case ClientNodeServiceState.Halted:
                    ConnectToBootNode();
                    break;
                case ClientNodeServiceState.ConnectingToBootNode:
                case ClientNodeServiceState.RetryingConnectionToBootNode:
                    break;
                case ClientNodeServiceState.Running:
                default:
                    break;
            }
        }

        public async void ConnectToBootNode()
        {
            if (ClientNodeServiceState.Halted == this.serviceState)
                this.serviceState = ClientNodeServiceState.RetryingConnectionToBootNode;
            else
                this.serviceState = ClientNodeServiceState.ConnectingToBootNode;

            NodeRPCClient client = new NodeRPCClient(Program.SystemOptions);
            if (true == client.ConnectToBootNode().Result)
                this.serviceState = ClientNodeServiceState.Running;
            else
                this.serviceState = ClientNodeServiceState.Halted;
        }
    }
}
