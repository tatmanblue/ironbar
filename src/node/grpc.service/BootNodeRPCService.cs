using Grpc.Core;
using Node.General; // Ensure you have the correct namespace here


namespace Node.grpc.service; 

/// <summary>
/// This is the bootnode listener for GRPC calls messages from child nodes.
/// this service only runs on the bootnode
/// </summary>
public class BootNodeRPCService : BootNode.BootNodeBase
{
    private readonly ILogger<BootNodeRPCService> logger;
    private ConnectionManager connectionManager;
    public BootNodeRPCService(ConnectionManager connectionManager, ILogger<BootNodeRPCService> logger)
    {
        this.connectionManager = connectionManager;
        this.logger = logger;
    }
    
    #region connection APIS
    public override Task<ConnectReply> Connect(ConnectRequest request, ServerCallContext context)
    {
        logger.LogInformation($"Connection established from {request.ClientAddr} {context.Host} {context.Method} {context.Peer}");
        connectionManager.AddNewChildNode(new ChildNodeConnection { Address = request.ClientAddr });
        return Task.FromResult(new ConnectReply
        {
            Message = $"Hello {request.FriendlyName}"
        });
    }
    
    public override Task<DisconnectReply> Disconnect(DisconnectRequest request, ServerCallContext context)
    {
        // TODO:
        logger.LogInformation($"Node disconnecting {request.ClientAddr}");
        connectionManager.RemoveChildNode(request.ClientAddr);
        return Task.FromResult(new DisconnectReply
        {
            Message = $"Goodbye {request.ClientAddr}"
        });
    }
    #endregion
    
}
