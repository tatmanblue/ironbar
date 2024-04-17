using core.Ledger;
using Grpc.Core;
using Node.General;
using Node.Interfaces;
using Node.Ledger; // Ensure you have the correct namespace here

namespace Node.grpc.service;

/// <summary>
/// IronBar consumers will create and retrieve blocks through BlockHandlingApi.proto definition
/// which is handled here.
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
            BlockHash = block.Hash,
            Status = block.Status.ToString()
        });
    }
    
    public override Task<ReadBlockReply> Read(ReadBlockRequest request, ServerCallContext context)
    {
        if (false == apiKeyManager.IsReadAllowed(request.ApiKey))
            throw new ApiKeyManagerException();        
        
        try 
        {
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
                BlockHash = pb.Hash,
                Status = pb.Status.ToString(),
                Nonce = pb.Nonce.ToString(),
                CreatedOn = pb.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss \"UTC\"zzz")
            };

            return Task.FromResult(result);
        }
        catch (FileNotFoundException)
        {
            throw new BlockChainException();
        }
    }
    
    public override Task<ListBlocksReply> List(ListBlocksRequest request, ServerCallContext context)
    {
        if (false == apiKeyManager.IsReadAllowed(request.ApiKey))
            throw new ApiKeyManagerException();

        try
        {
            bool readMessage = apiKeyManager.IsReadDetailsAllowed(request.ApiKey) && request.IncludeBody;

            ListBlocksReply reply = new ListBlocksReply();
            List<ILedgerIndex> indexes = ledgerManager.ListAllBlocks();
            foreach (ILedgerIndex entry in indexes)
            {
                string blockData = string.Empty;
                if (readMessage)
                {
                    ILedgerPhysicalBlock pb = ledgerManager.GetBlock(entry.BlockId);
                    blockData = System.Text.Encoding.Default.GetString(pb.TransactionData);
                }

                reply.Blocks.Add(new Block()
                {
                    BlockId = entry.BlockId.ToString(),
                    BlockHash = entry.Hash,
                    Status = entry.Status.ToString(),
                    BlockData = blockData,
                });
            }

            return Task.FromResult(reply);
        }
        catch 
        {
            throw new BlockChainSystemException("List Blocks failed");
        }
    }
}