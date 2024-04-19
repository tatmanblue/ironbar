using System.Net;
using System.Net.Sockets;
using core.Ledger;
using core.Security;
using core.Utility;
using Grpc.Net.Client;
using Node.General;
using Node.Interfaces;
using IConfiguration = core.IConfiguration;

namespace Node.grpc.service;

/// <summary>
/// TODO this service is not part of the GRPC implementation but it does consume GRPC implementations
/// 
/// BootNodeRPCClient handles messages to child nodes from the boot node
/// </summary>
public class BootNodeRPCClient : IHostedService, IDisposable
{
    #region IHostedService, IDisposable
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("BootNodeRPCClient is initializing.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("BootNodeRPCClient is stopping.");
        return Task.CompletedTask;
    }
    
    public void Dispose()
    {
    }
    #endregion
    
    private readonly ILogger<BootNodeRPCClient> logger;
    private readonly IConfiguration options;
    private readonly ILedgerManager ledgerManager;
    private readonly ConnectionManager connectionManager;

    
    public BootNodeRPCClient(IServiceProvider serviceProvider)
    {
        this.logger = serviceProvider.GetRequiredService<ILogger<BootNodeRPCClient>>();
        this.ledgerManager = serviceProvider.GetRequiredService<ILedgerManager>();
        this.options = serviceProvider.GetRequiredService<IConfiguration>();
        this.connectionManager = serviceProvider.GetRequiredService<ConnectionManager>();
        
        IServicesEventSub eventSub = serviceProvider.GetRequiredService<IServicesEventSub>();
        
        eventSub.OnBlockCreated += OnBlockCreated;
        eventSub.OnChildNodeConnected += OnChildNodeCreated;
        eventSub.OnShutdown += OnNotifyNodeOfShutdown;
    }
    
    private void OnChildNodeCreated(ChildNodeConnection conn)
    {
        logger.LogInformation("BootNodeRPCClient is handling a new node");
        var channel = GrpcChannel.ForAddress(conn.Address);
        var client = new NodeToNodeConnection.NodeToNodeConnectionClient(channel);
        IndexRequest request = new IndexRequest();
        List<ILedgerIndex> indexes = ledgerManager.ListAllBlocks();
        string runningHash = string.Empty;
        
        foreach (ILedgerIndex idx in indexes)
        {
            request.Indexes.Add(idx.ToString());
            runningHash = HashUtility.ComputeHash($"{runningHash}{idx.Hash}");
        }

        request.Verification = runningHash;
        client.SyncIndex(request);
        logger.LogInformation("Sent index to node");
    }

    private void OnBlockCreated(ILedgerPhysicalBlock pb)
    {
        logger.LogInformation("BootNodeRPCClient is sharing new block");
        List<Task> tasks = new();
        
        foreach (ChildNodeConnection conn in connectionManager.ActiveConnections)
        {
            tasks.Add(
                Task.Run(() => SendBlockToChildNode(pb, conn))
            );
        }
        
        logger.LogInformation($"Shared new block with {tasks.Count} nodes ");
        Task.WhenAll(tasks).ContinueWith(done =>
        {
            logger.LogDebug("Child nodes notified of new block");
        });
    }

    private void SendBlockToChildNode(ILedgerPhysicalBlock pb, ChildNodeConnection conn)
    {
        var channel = GrpcChannel.ForAddress(conn.Address);
        var client = new NodeToNodeConnection.NodeToNodeConnectionClient(channel);
        BlockCreatedRequest blockCreatedRequest = new BlockCreatedRequest()
        {
            Block = pb.ToString(),
            Verification = pb.Hash
        };
        var empty = client.BlockCreated(blockCreatedRequest);
        // TODO if client rejects the ledger, it means there is a synchronization problem. what should happen? 
    }

    private void OnNotifyNodeOfShutdown(ChildNodeConnection clientNode)
    {
        // TODO we can refactor this but saving it for later
        Task.Run(() =>
        {
            bool result = SendShuttingDownMessage(clientNode).Result;
        });
    }

    public async Task<bool> SendShuttingDownMessage(ChildNodeConnection clientNode)
    {
        try
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(clientNode.Address);
            var client = new NodeToNodeConnection.NodeToNodeConnectionClient(channel);
            var reply = client.Disconnect(new DisconnectRequest() 
                { ClientAddr = options.BootAddress, FriendlyName = options.FriendlyName});
            logger.LogInformation("SendShuttingDownMessage reply: " + reply.Message);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError($"ERROR: {ex.Message}");
            if (ex.InnerException != null)
                logger.LogInformation($"INNER EXCEPTION: {ex.InnerException}");
            else
                logger.LogInformation("no more details");

            return false;
        }
    }


}
