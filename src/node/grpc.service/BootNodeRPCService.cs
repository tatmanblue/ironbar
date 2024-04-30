using core.Ledger;
using core.Security;
using core.Utility;
using Grpc.Core;
using Node.General;
using Node.Interfaces;
using Node.Ledger; // Ensure you have the correct namespace here


namespace Node.grpc.service;

/// <summary>
/// This is the bootnode listener for GRPC calls messages from child nodes.
/// this service only runs on the bootnode
///
/// The counter part for this, aka a listener for GRPC calls from bootnode, is
/// the ChildNodeRPCService implementation
/// </summary>
public class BootNodeRPCService : NodeToNodeConnection.NodeToNodeConnectionBase
{
    private readonly ILogger<BootNodeRPCService> logger;
    private readonly IServicesEventPub eventPub;
    private readonly ILedgerManager ledgerManager;
    private ConnectionManager connectionManager;
    
    public BootNodeRPCService(IServiceProvider serviceProvider)
    {
        this.connectionManager = serviceProvider.GetRequiredService<ConnectionManager>();
        this.logger = serviceProvider.GetRequiredService<ILogger<BootNodeRPCService>>();
        this.eventPub = serviceProvider.GetRequiredService<IServicesEventPub>();
        this.ledgerManager = serviceProvider.GetRequiredService<ILedgerManager>();
        logger.LogInformation($"BootNodeRPCService initialized.");
    }

    #region connection APIS
    public override Task<ConnectReply> Connect(ConnectRequest request, ServerCallContext context)
    {
        logger.LogInformation($"Connection established from {request.ClientAddr} {context.Host} {context.Method} {context.Peer}");

        ChildNodeConnection connection = new ChildNodeConnection
        {
            Address = request.ClientAddr, 
            Name = request.FriendlyName,
            Version = request.NodeVersion,
            State = ChildNodeState.Requested
        };
        
        logger.LogDebug("Adding new connection in connection manager");
        connectionManager.AddNewChildNode(connection);
        
        logger.LogWarning("WILL NOT! Firing client connected event");
        // eventPub.FireClientConnected(connection);
        
        logger.LogDebug("Sending reply");
        return Task.FromResult(new ConnectReply
        {
            Message = $"Hello {request.FriendlyName}"
        });
    }

    public override Task<IndexResponse> ClientRequestIndex(ConnectRequest request, ServerCallContext context)
    {
        try
        {

            connectionManager.SetChildState(request.FriendlyName, ChildNodeState.Initializing);
            
            List<ILedgerIndex> indexes = ledgerManager.ListAllBlocks();

            IndexResponse response = new IndexResponse();
            response.Nonce = Nonce.New().ToString();
            foreach (ILedgerIndex idx in indexes)
            {
                response.Indexes.Add(idx.ToString());
            }

            return Task.FromResult(response);
        }
        catch (ConnectionManagerException e)
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, $"Call not valid for {request.FriendlyName}/{request.ClientAddr}"));
        }
    }

    public override Task<ValidateIndexResponse> ClientValidatesIndex(ValidateIndexRequest request, ServerCallContext context)
    {
        try
        {
            List<ILedgerIndex> indexes = ledgerManager.ListAllBlocks();
            string runningHash = string.Empty;
        
            foreach (ILedgerIndex idx in indexes)
            {
                runningHash = HashUtility.ComputeHash($"{runningHash}{idx.Hash}");
            }

            if (runningHash != request.Validation)
                throw new BlockChainException();
            
            connectionManager.SetChildState(request.FriendlyName, ChildNodeState.Allowed);
            ValidateIndexResponse response = new()
            {
                Approved = Globals.APPROVED
            };

            return Task.FromResult(response);
        }
        catch (ConnectionManagerException e)
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, $"Call not valid for {request.FriendlyName}/{request.ClientAddr}"));
        }
    }

    public override Task<DisconnectReply> Disconnect(DisconnectRequest request, ServerCallContext context)
    {
        logger.LogInformation($"Node disconnecting {request.ClientAddr}");
        connectionManager.RemoveChildNode(request.FriendlyName);
        return Task.FromResult(new DisconnectReply
        {
            Message = $"Goodbye {request.FriendlyName} {request.ClientAddr}"
        });
    }
    #endregion
    
}
