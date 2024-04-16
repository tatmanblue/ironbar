using IConfiguration = core.IConfiguration;

namespace Node.grpc.service; 

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
/// It is used to keep in touch with the boot node, adjusting state according to connectivity with bootnode
/// </summary>
public class ChildNodeService : IHostedService, IDisposable
{
    
    #region IHostedService
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("ClientNodeService is initializing.");

        doWorkDelay = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(INTERVAL));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("ClientNodeService is stopping.");
        serviceState = ChildNodeServiceState.ShuttingDown;
        doWorkDelay.Change(Timeout.Infinite, 0);

        // send message to bootnode this node is going off line
        rpcClient.SendShuttingDownMessage();

        return Task.CompletedTask;
    }
    #endregion
    
    private const int INTERVAL = 5;

    private readonly ILogger<ChildNodeService> logger;
    private readonly IConfiguration options;
    private readonly ChildNodeRPCClient rpcClient;

    private Timer doWorkDelay;
    private ChildNodeServiceState serviceState = ChildNodeServiceState.NotStarted;

    public ChildNodeService(ChildNodeRPCClient rpcClient, IConfiguration options, ILogger<ChildNodeService> logger)
    {
        this.logger = logger;
        this.options = options;
        this.rpcClient = rpcClient;

    }

    public void Dispose()
    {
        doWorkDelay?.Dispose();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="state"></param>
    private void DoWork(object state)
    {
        switch (serviceState)
        {
            case ChildNodeServiceState.NotStarted:
            case ChildNodeServiceState.Halted:
                logger.LogInformation($"ClientNodeService is state {serviceState}");
                ConnectToBootNode();
                break;
            case ChildNodeServiceState.ConnectingToBootNode:
            case ChildNodeServiceState.RetryingConnectionToBootNode:
                logger.LogInformation($"ClientNodeService is state {serviceState}");
                break;
            case ChildNodeServiceState.Running:
                break;
            case ChildNodeServiceState.ShuttingDown:
                logger.LogInformation($"ClientNodeService is state {serviceState}");
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

        if (true == await rpcClient.ConnectToBootNode())
            this.serviceState = ChildNodeServiceState.Running;
        else
            this.serviceState = ChildNodeServiceState.Halted;
    }
}
