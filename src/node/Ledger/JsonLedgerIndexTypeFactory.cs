
using core.Ledger;
using Newtonsoft.Json;

namespace Node.Ledger;

public class JsonLedgerIndexTypeFactory : ILedgerIndexFactory
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
        return LedgerIndex.FromJson(data);
    }
    
    public ILedgerIndex Create() => new LedgerIndex();
    
    public List<ILedgerIndex> CreateList(string data)
    {
        var settings = new JsonSerializerSettings
        {
            Converters = { new LedgerIndexConverter(this) },
            Formatting = Formatting.Indented
        };
        var ledgerIndices = JsonConvert.DeserializeObject<List<ILedgerIndex>>(data, settings);
        return ledgerIndices;
    }
}