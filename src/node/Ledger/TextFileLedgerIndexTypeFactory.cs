using core.Ledger;

namespace Node.Ledger;

public class TextFileLedgerIndexTypeFactory : ILedgerIndexFactory
{
    public ILedgerIndex Create(int blockId, string hash, DateTime created, BlockStatus status)
    {
        LedgerIndex index = new LedgerIndex();
        index.BlockId = blockId;
        index.Created = created;
        index.Hash = hash;
        index.Status = status;

        return index;
    }

    public ILedgerIndex Create(string data)
    {
        return LedgerIndex.FromString(data);
    }
}