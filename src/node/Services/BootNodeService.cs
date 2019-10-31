using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;


namespace node
{
    public class BootNodeService : BootNode.BootNodeBase
    {
        private readonly ILogger<BootNodeService> _logger;
        public BootNodeService(ILogger<BootNodeService> logger)
        {
            _logger = logger;
        }

        public override Task<LinkReply> AddLink(LinkRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"got a SayHello from {request.ClientAddr}");
            return Task.FromResult(new LinkReply
            {
                Message = "Hello " + request.ClientAddr
            });
        }

    }
}
