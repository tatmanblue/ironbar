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
        
        ILedgerPhysicalBlock block = ledgerManager.Create(request.BlockData);

        logger.LogInformation($"Block created {block.Id}");
        
        return Task.FromResult(new CreateBlockReply
        {
            BlockId = block.Id.ToString(),
            BlockHash = block.Hash
        });
    }
    
    public override Task<ReadBlockReply> Read(ReadBlockRequest request, ServerCallContext context)
    {
        if (false == apiKeyManager.IsReadAllowed(request.ApiKey))
            throw new ApiKeyManagerException();        
        
        bool readMessage = apiKeyManager.IsReadDetailsAllowed(request.ApiKey);
        ILedgerPhysicalBlock pb = ledgerManager.GetBlock(Convert.ToInt32(request.BlockId));
        string blockData = string.Empty;
        if (readMessage)
        {
            blockData =  System.Text.Encoding.Default.GetString(pb.TransactionData);
        }

        ReadBlockReply result = new ReadBlockReply()
        {
            BlockId = pb.Id.ToString(),
            BlockData = blockData,
            BlockHash = pb.Hash
        };

        return Task.FromResult(result);
    }
    
    public override Task<ListBlocksReply> List(ListBlocksRequest request, ServerCallContext context)
    {
        if (false == apiKeyManager.IsReadAllowed(request.ApiKey))
            throw new ApiKeyManagerException();
        
        bool readMessage = apiKeyManager.IsReadDetailsAllowed(request.ApiKey) && request.IncludeBody;
        
        ListBlocksReply reply = new ListBlocksReply();
        List<ILedgerIndex> indexes = ledgerManager.ListAllBlocks();
        foreach (ILedgerIndex entry in indexes)
        {
            string blockData = string.Empty;
            if (readMessage)
            {
                ILedgerPhysicalBlock pb = ledgerManager.GetBlock(entry.BlockId);
                blockData =  System.Text.Encoding.Default.GetString(pb.TransactionData);
            }
            
            reply.Blocks.Add(new Block()
            {
                BlockId = entry.BlockId.ToString(),
                BlockHash = entry.Hash,
                BlockData = blockData
            });
        }
        
        return Task.FromResult(reply);
    }
}