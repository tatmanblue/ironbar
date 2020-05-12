using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using node.General;

namespace node.Services
{
    public enum ChildNodeServiceState
    {
        NotStarted,
        ConnectingToBootNode,
        Running,
        ShuttingDown,
        Halted,
        RetryingConnectionToBootNode,
    }

    public class ChildNodeServiceException : Exception
    {
        public ChildNodeServiceException(string message) : base(message) { }
    }

    /// <summary>
    /// When a node is a client, this service starts up and runs as long as the service is functional
    /// it is used to maintain and update state
    /// </summary>
    public class ChildNodeService : IHostedService, IDisposable
    {
        private const int INTERVAL = 5;

        private readonly ILogger<ChildNodeService> _logger;
        private readonly IOptions _options;
        private readonly ChildNodeRPCClient _rpcClient;

        private Timer doWorkDelay;
        private ChildNodeServiceState serviceState = ChildNodeServiceState.NotStarted;

        public ChildNodeService(ChildNodeRPCClient rpcClient, IOptions options, ILogger<ChildNodeService> logger)
        {
            _logger = logger;
            _options = options;
            _rpcClient = rpcClient;

        }

        public void Dispose()
        {
            doWorkDelay?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ClientNodeService is initializing.");

            doWorkDelay = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(INTERVAL));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ClientNodeService is stopping.");
            serviceState = ChildNodeServiceState.ShuttingDown;
            doWorkDelay.Change(Timeout.Infinite, 0);

            // send message to bootnode this node is going off line
            _rpcClient.SendShuttingDownMessage();

            return Task.CompletedTask;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="state"></param>
        private void DoWork(object state)
        {
            _logger.LogInformation($"ClientNodeService is state {serviceState}");

            switch (serviceState)
            {
                case ChildNodeServiceState.NotStarted:
                case ChildNodeServiceState.Halted:
                    ConnectToBootNode();
                    break;
                case ChildNodeServiceState.ConnectingToBootNode:
                case ChildNodeServiceState.RetryingConnectionToBootNode:
                    break;
                case ChildNodeServiceState.Running:
                    break;
                case ChildNodeServiceState.ShuttingDown:
                    break;
                default:
                    throw new ChildNodeServiceException("an invalid state was encountered in ClientNodeService");
            }
        }

        public async void ConnectToBootNode()
        {
            if (ChildNodeServiceState.Halted == this.serviceState)
                this.serviceState = ChildNodeServiceState.RetryingConnectionToBootNode;
            else
                this.serviceState = ChildNodeServiceState.ConnectingToBootNode;

            if (true == await _rpcClient.ConnectToBootNode())
                this.serviceState = ChildNodeServiceState.Running;
            else
                this.serviceState = ChildNodeServiceState.Halted;
        }
    }
}
