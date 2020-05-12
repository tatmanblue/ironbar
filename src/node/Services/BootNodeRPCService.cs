using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using node.General;
using static node.BootNode;

namespace node
{
    // This is the bootnode listener for GRPC calls messages from child nodes.
    // this service only runs on the bootnood
    public class BootNodeRPCService : BootNodeBase
    {
        private readonly ILogger<BootNodeRPCService> _logger;
        private ConnectionManager _connectionManager;
        public BootNodeRPCService(ConnectionManager connectionManager, ILogger<BootNodeRPCService> logger)
        {
            _connectionManager = connectionManager;
            _logger = logger;
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
            _logger.LogInformation($"got a SayHello from {request.ClientAddr} {context.Host} {context.Method} {context.Peer}");
            _connectionManager.AddNewChildNode(new ChildNodeConnection { Address = request.ClientAddr });
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
            _logger.LogInformation($"got a SimpleMessage from {request.ClientAddr}");
            _connectionManager.RemoveChildNode(request.ClientAddr);
            return Task.FromResult(new SimpleMessageReply
            {
                Message = "Goodbye " + request.ClientAddr
            });
        }
    }
}
