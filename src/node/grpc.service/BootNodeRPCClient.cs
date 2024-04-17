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
public class BootNodeRPCClient
{
    private readonly ILogger<BootNodeRPCClient> logger;
    private readonly IConfiguration options;
    private readonly ILedgerManager ledgerManager;

    public BootNodeRPCClient(ILedgerManager ledgerManager, IConfiguration options, ILogger<BootNodeRPCClient> logger, IServicesEventSub eventSub)
    {
        this.ledgerManager = ledgerManager;
        this.options = options;
        this.logger = logger;
        
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

        /*
        foreach (ChildNodeConnection conn in COLLECTION)
        {
            tasks.Add(
                Task.Run(() => SendBlockToClient(pb, conn))
            );
        }
        */

        Task.WhenAll(tasks).ContinueWith(done =>
        {
            logger.LogDebug("Child nodes notified of new block");
        });
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
