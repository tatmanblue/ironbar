using core.Ledger;

namespace Node.Ledger;

public class TextFilePhysicalBlockTypeFactory : ILedgerPhysicalBlockFactory
{
    /// <summary>
    /// Creates an original block submission with no reference to a prior block.
    /// ReferenceId is set to 0 (sentinel for "no reference").
    /// Used by Ledger.AddBlock when a new block is submitted by a client.
    /// </summary>
    public ILedgerPhysicalBlock Create(int id, BlockStatus status, string parentHash, int parentId, int ledgerId,
        byte[] transactionData, ILedgerSignBlock signBlock)
    {
        PhysicalBlock block = new PhysicalBlock(status)
        {
            Id = id,
            ParentHash = parentHash,
            ParentId = parentId,
            ReferenceId = 0,    // no reference — this is an original block submission
            LedgerId = ledgerId,
            TransactionData = transactionData,
            SignBlock = signBlock
        };

        return block;
    }

    /// <summary>
    /// Creates a block that advances a prior block (e.g. Unconfirmed → Confirmed in the BFT flow).
    /// referenceId and referenceHash identify the earlier block being superseded; the validator
    /// enforces that referenceHash matches the referenced block's hash.
    /// Used by Ledger.AdvanceBlock.
    /// </summary>
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

    /// <summary>
    /// Deserializes a block from its colon-delimited string representation (as produced by ToString()).
    /// Used when receiving blocks over the wire from the boot node or reading from disk.
    /// </summary>
    public ILedgerPhysicalBlock Create(string block, ILedgerSignBlockFactory signBlockFactory)
    {
        return Create(block);
    }

    public ILedgerPhysicalBlock Create(string block)
    {
        return PhysicalBlock.FromString(block);
    }
    
}
