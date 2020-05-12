using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static node.BootNode;

namespace node
{
    // This is the bootnode listener for GRPC calls messages from child nodes.
    // this service only runs on the bootnood
    public class BootNodeService : BootNodeBase
    {
        private readonly ILogger<BootNodeService> _logger;
        public BootNodeService(ILogger<BootNodeService> logger)
        {
            _logger = logger;
        }

        public override Task<LinkReply> AddLink(LinkRequest request, ServerCallContext context)
        {
            // TODO:
            _logger.LogInformation($"got a SayHello from {request.ClientAddr}");
            return Task.FromResult(new LinkReply
            {
                Message = "Hello " + request.ClientAddr
            });
        }

        public override Task<SimpleMessageReply> SendSimpleMessage(SimpleMessage request, ServerCallContext context)
        {
            // TODO:
            _logger.LogInformation($"got a SimpleMessage from {request.ClientAddr}");
            return Task.FromResult(new SimpleMessageReply
            {
                Message = "Goodbye " + request.ClientAddr
            });
        }
    }
}
