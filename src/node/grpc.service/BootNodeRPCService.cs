using Grpc.Core;
using Node.General;
using Node.Interfaces; // Ensure you have the correct namespace here


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
    private ConnectionManager connectionManager;
    
    public BootNodeRPCService(ConnectionManager connectionManager, ILogger<BootNodeRPCService> logger, IServicesEventPub eventPub)
    {
        this.connectionManager = connectionManager;
        this.logger = logger;
        this.eventPub = eventPub;
    }

    #region connection APIS
    public override Task<ConnectReply> Connect(ConnectRequest request, ServerCallContext context)
    {
        logger.LogInformation($"Connection established from {request.ClientAddr} {context.Host} {context.Method} {context.Peer}");

        ChildNodeConnection connection = new ChildNodeConnection
        {
            Address = request.ClientAddr, 
            Name = request.FriendlyName,
            Version = request.NodeVersion
        };
        connectionManager.AddNewChildNode(connection);
        
        eventPub.FireClientConnected(connection);
        
        return Task.FromResult(new ConnectReply
        {
            Message = $"Hello {request.FriendlyName}"
        });
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
