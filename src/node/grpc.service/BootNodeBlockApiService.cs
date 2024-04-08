using core.Ledger;
using Grpc.Core;
using Node.General;
using Node.Interfaces;
using Node.Ledger; // Ensure you have the correct namespace here

namespace Node.grpc.service;

/// <summary>
/// 
/// </summary>
public class BootNodeBlockApiService : BlockHandlingApi.BlockHandlingApiBase
{
    private readonly ILogger<BootNodeBlockApiService> logger;
    private readonly ILedgerManager ledgerManager;
    private readonly ApiKeyManager apiKeyManager;
    
    public BootNodeBlockApiService(ILogger<BootNodeBlockApiService> logger, ApiKeyManager apiKeyManager, ILedgerManager ledgerManager)
    {
        this.logger = logger;
        this.apiKeyManager = apiKeyManager;
        this.ledgerManager = ledgerManager;
    }

    public override Task<CreateBlockReply> Create(CreateBlockRequest request, ServerCallContext context)
    {
        if (false == apiKeyManager.IsWriteAllowed(request.ApiKey))
            throw new ApiKeyManagerException();
        throw new NotImplementedException();
    }
    
    public override Task<ReadBlockReply> Read(ReadBlockRequest request, ServerCallContext context)
    {
        if (false == apiKeyManager.IsReadAllowed(request.ApiKey))
            throw new ApiKeyManagerException();        
        throw new NotImplementedException();
    }
    
    public override Task<ListBlocksReply> List(ListBlocksRequest request, ServerCallContext context)
    {
        if (false == apiKeyManager.IsReadAllowed(request.ApiKey))
            throw new ApiKeyManagerException();

        ListBlocksReply reply = new ListBlocksReply();
        List<ILedgerIndex> indexes = ledgerManager.ListAllBlocks();
        foreach (ILedgerIndex entry in indexes)
        {
            reply.Blocks.Add(new Block()
            {
                BlockId = entry.BlockId.ToString(),
                BlockHash = entry.Hash
            });
        }
        
        return Task.FromResult(reply);
    }
}