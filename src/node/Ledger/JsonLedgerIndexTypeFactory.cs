using core.Ledger;

namespace Node.Ledger;

public class JsonLedgerIndexTypeFactory : ILedgerIndexFactory
{
    public ILedgerIndex CreateLedgerIndex(int blockId, string hash, DateTime created, BlockStatus status)
    {
        LedgerIndex index = new LedgerIndex();
        index.BlockId = blockId;
        index.Created = created;
        index.Hash = hash;
        index.Status = status;

        return index;
    }

    public ILedgerIndex CreateLedgerIndex(string data)
    {
        return LedgerIndex.FromJson(data);
    }
}