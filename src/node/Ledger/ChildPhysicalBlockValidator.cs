using core.Ledger;

namespace Node.Ledger;

public class ChildPhysicalBlockValidator : IPhysicalBlockValidator
{
    public void Validate(ILedgerIndexManager indexes, ILedgerPhysicalBlock block)
    {
        ILedgerIndex lastParentIndex = indexes.GetIndex(block.Id - 1);
            
        if (block.ParentHash != lastParentIndex.Hash)
            throw new LedgerNotValidException($"Invalid Chain. block {block.Id}");
        if (block.ParentId != lastParentIndex.BlockId)
            throw new LedgerNotValidException($"Invalid Chain. block {block.Id}");
    }
}