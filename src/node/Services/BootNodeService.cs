using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;


namespace node
{
    // keeps track of which nodes are part of the system.  this service only runs on the bootnood
    public class BootNodeService : BootNode.BootNodeBase
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

    }
}
