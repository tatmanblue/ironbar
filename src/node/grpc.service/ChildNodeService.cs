﻿using IConfiguration = core.IConfiguration;

namespace Node.grpc.service; 


/// <summary>
/// TODO this service is not part of the GRPC implementation but it does consume GRPC implementations
///
/// It is effectively a state manager for a client node.  When a node is a client, this service starts up
/// and runs as long as the service is functional and adjusts state based on its ability to communicate with
/// the bootnode. 
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
        logger.LogInformation($"ClientNodeService is stopping. {serviceState}");
        doWorkDelay.Change(Timeout.Infinite, 0);

        if (serviceState != ChildNodeServiceState.Running)
        {
            serviceState = ChildNodeServiceState.ShuttingDown;
            return Task.CompletedTask;
        }

        serviceState = ChildNodeServiceState.ShuttingDown;
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
