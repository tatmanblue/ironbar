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
    
    public ILedgerIndex Create() => new LedgerIndex();

    /// <summary>
    /// Currently not supporting this send DefaultTextFileLedgerReader does not use it (yet)
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public List<ILedgerIndex> CreateList(string data)
    {
        throw new NotImplementedException();
    }
}