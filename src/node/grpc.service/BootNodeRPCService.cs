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

    /// <summary>
    /// Handles a "hello" from a child node
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override Task<LinkReply> AddLink(LinkRequest request, ServerCallContext context)
    {
        // TODO:
        logger.LogInformation($"got a SayHello from {request.ClientAddr} {context.Host} {context.Method} {context.Peer}");
        connectionManager.AddNewChildNode(new ChildNodeConnection { Address = request.ClientAddr });
        return Task.FromResult(new LinkReply
        {
            Message = "Hello " + request.ClientAddr
        });
    }

    /// <summary>
    /// Handles SimpleMessage from child nodes
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override Task<SimpleMessageReply> SendSimpleMessage(SimpleMessage request, ServerCallContext context)
    {
        // TODO:
        logger.LogInformation($"got a SimpleMessage from {request.ClientAddr}");
        connectionManager.RemoveChildNode(request.ClientAddr);
        return Task.FromResult(new SimpleMessageReply
        {
            Message = "Goodbye " + request.ClientAddr
        });
    }
}
