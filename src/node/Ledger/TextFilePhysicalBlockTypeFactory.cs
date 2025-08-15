using core.Ledger;

namespace Node.Ledger;

public class TextFilePhysicalBlockTypeFactory : ILedgerPhysicalBlockFactory
{
    public ILedgerPhysicalBlock Create(int id, BlockStatus status, string parentHash, int parentId, int ledgerId,
        byte[] transactionData, ILedgerSignBlock signBlock)
    {
        PhysicalBlock block = new PhysicalBlock(status)
        {
            Id = id,
            ParentHash = parentHash,
            ParentId = parentId,
            LedgerId = ledgerId,
            TransactionData = transactionData,
            SignBlock = signBlock
        };
        
        return block;
    }

    public ILedgerPhysicalBlock Create(int id, BlockStatus status, string parentHash, int parentId, int referenceId,
        string referenceHash, int ledgerId, byte[] transactionData, ILedgerSignBlock signBlock)
    {
        PhysicalBlock block = new PhysicalBlock(status)
        {
            Id = id,
            ParentHash = parentHash,
            ParentId = parentId,
            ReferenceId = referenceId,
            ReferenceHash = referenceHash,
            LedgerId = ledgerId,
            TransactionData = transactionData,
            SignBlock = signBlock
        };
        
        return block;
    }
    
    public ILedgerPhysicalBlock Create(string block)
    {
        return PhysicalBlock.FromString(block);
    }

    /*
    public ILedgerSignBlock CreateSignBlock(string block)
    {
        throw new NotImplementedException();
    }
    */
}
