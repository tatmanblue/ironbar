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
    /// <summary>
    /// Child nodes connecting to the boot node now have a multi-step process. "Connect" is
    /// first which allows for the other steps to proceed.  Child nodes must get through all
    /// steps successfully in order be allowed to validate blocks
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
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
        
        logger.LogDebug("Sending reply");
        return Task.FromResult(new ConnectReply
        {
            Message = $"Hello {request.FriendlyName}"
        });
    }

    /// <summary>
    /// Child nodes connecting to the boot node now have a multi-step process.  This step
    /// is part of 2 messages of validating the index.  The child node requests the index file
    /// (and will be expected to send a validation message next)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="RpcException"></exception>
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

    /// <summary>
    /// Child nodes connecting to the boot node now have a multi-step process.  This step
    /// is second part of 2 messages of validating the index.  The child node is sending its validation
    /// of the index.  If the boot node accepts the child node validation, the child node will be
    /// enabled to validate blocks in the system
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="BlockChainException"></exception>
    /// <exception cref="RpcException"></exception>
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

            // the child node validation did not match that of the boot node therefore it is assumed
            // the child node is not a good player in the system and will not allowed to continue.
            // There is a legit possibility this could happen if a new block comes into the system while this
            // child was validating the index (which would be "old" at this point).
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
