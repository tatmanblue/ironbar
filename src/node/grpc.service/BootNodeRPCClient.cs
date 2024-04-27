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
        // Task.Delay(2000).Wait();
        logger.LogInformation($"BootNodeRPCClient is handling a new node {conn.Name} using {conn.Address}");
        IndexRequest request = new IndexRequest();
        List<ILedgerIndex> indexes = ledgerManager.ListAllBlocks();
        string runningHash = string.Empty;
        
        foreach (ILedgerIndex idx in indexes)
        {
            request.Indexes.Add(idx.ToString());
            runningHash = HashUtility.ComputeHash($"{runningHash}{idx.Hash}");
        }

        request.Verification = runningHash;
        
        var channel = GrpcChannel.ForAddress(conn.Address);
        var client = new NodeToNodeConnection.NodeToNodeConnectionClient(channel);
        logger.LogDebug($"Channel created for {conn.Name}");
        client.SyncIndex(request);
        logger.LogInformation($"Sent index to node {conn.Name}");
    }

    private void OnBlockCreated(ILedgerPhysicalBlock pb)
    {
        logger.LogInformation($"BootNodeRPCClient is sharing new block {pb.Id}");
        List<Task> tasks = new();
        int nodesAcceptingBlock = 0;
        int maxAccepting = connectionManager.ActiveConnections.Count;

        if (0 == maxAccepting)
        {
            if (pb.Status == BlockStatus.Unconfirmed)
                // when there are no child nodes to validate, the status will be confirmed
                // which indicates boot node accepted the block and theres been no additional validation
                ledgerManager.AdvanceBlock(pb, BlockStatus.Confirmed);
            return;
        }
        
        foreach (ChildNodeConnection conn in connectionManager.ActiveConnections)
        {
            tasks.Add(
                Task.Run(() =>
                {
                    Interlocked.Add(ref nodesAcceptingBlock, SendBlockToChildNode(pb, conn));
                })
            );
        }
        
        logger.LogInformation($"Sharing new block with {tasks.Count} nodes ");
        Task.WhenAll(tasks).ContinueWith(done =>
        {
            logger.LogDebug("All nodes have replied");
        }).Wait();
        
        logger.LogInformation($"{maxAccepting} child nodes notified of new block, accepting nodes {nodesAcceptingBlock}");
        
        // only handle block advancement when input block started as unconfirmed
        if (pb.Status != BlockStatus.Unconfirmed)
            return;
        
        int bftPercentRequired = EnvironmentUtility.FromEnvOrDefaultAsInt("IRONBAR_BFT_PERCENT", "50");
        double bftCompute = (double)nodesAcceptingBlock / maxAccepting;
        int bftAcceptance = (int) Math.Floor(bftCompute * 100);

        logger.LogDebug($"BFT computations > bftAcceptance:{bftAcceptance} >= bftPercentRequired:{bftPercentRequired} [bftCompute: {bftCompute}]");
        if (bftAcceptance >= bftPercentRequired)
        {
            ledgerManager.AdvanceBlock(pb, BlockStatus.Approved);
        }
        else
        {
            logger.LogCritical($"Not enough child nodes accepted block {pb.Id} to accept block");
            ledgerManager.AdvanceBlock(pb, BlockStatus.Rejected);
        }
    }

    private int SendBlockToChildNode(ILedgerPhysicalBlock pb, ChildNodeConnection conn)
    {
        var channel = GrpcChannel.ForAddress(conn.Address);
        var client = new NodeToNodeConnection.NodeToNodeConnectionClient(channel);
        BlockCreatedRequest blockCreatedRequest = new BlockCreatedRequest()
        {
            Block = pb.ToString(),
            Verification = pb.Hash
        };
        BlockCreatedReply reply = client.BlockCreated(blockCreatedRequest);
        BlockStatus replyStatus = Enum.Parse<BlockStatus>(reply.Result);
        if (pb.Status == BlockStatus.Unconfirmed && replyStatus == BlockStatus.Approved)
            return 1;

        return 0;
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
