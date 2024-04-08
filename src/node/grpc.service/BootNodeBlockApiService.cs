using Grpc.Core;
using Node.General; // Ensure you have the correct namespace here

namespace Node.grpc.service;

/// <summary>
/// 
/// </summary>
public class BootNodeBlockApiService : BlockHandlingApi.BlockHandlingApiBase
{
    private readonly ILogger<BootNodeBlockApiService> logger;
    private readonly ApiKeyManager apiKeyManager;
    
    public BootNodeBlockApiService(ILogger<BootNodeBlockApiService> logger, ApiKeyManager apiKeyManager)
    {
        this.logger = logger;
        this.apiKeyManager = this.apiKeyManager;
    }

    public override Task<CreateBlockReply> Create(CreateBlockRequest request, ServerCallContext context)
    {
        throw new NotImplementedException();
    }
    
    public override Task<ReadBlockReply> Read(ReadBlockRequest request, ServerCallContext context)
    {
        throw new NotImplementedException();
    }
    
    public override Task<ListBlocksReply> List(ListBlocksRequest request, ServerCallContext context)
    {
        throw new NotImplementedException();
    }
}