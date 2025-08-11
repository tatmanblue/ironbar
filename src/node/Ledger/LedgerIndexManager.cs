using core.Ledger;
using Node.Interfaces;

namespace Node.Ledger;

/// <summary>
/// Ledger index is a simplified collection of data about
/// ledger blocks (currently physical blocks).  The manager
/// provides the functionality for accessing the data
/// </summary>
public class LedgerIndexManager : ILedgerIndexManager
{
    public ILedgerWriter Writer { get; private set; }
    public ILedgerReader Reader { get; private set; }
    private List<ILedgerIndex> data = new List<ILedgerIndex>();
    private bool isLoaded = false;

    public string IndexFile { get; private set; }
    public string LedgerName { get; private set; }

    public LedgerIndexManager(string name, string ledgerPath, string indexFile)
    {
        IndexFile = indexFile;
        LedgerName = name;
        Writer = new DefaultTextFileLedgerWriter(ledgerPath);
        Reader =  new DefaultTextFileLedgerReader(ledgerPath);
    }

    public ILedgerIndex Add(string hash, DateTime created, BlockStatus status)
    {
        if (false == isLoaded)
            throw new LedgerException(LedgerName, "Attempted to add index before loading");

        LedgerIndex index = new LedgerIndex();
        index.BlockId = GetNextBlockId();
        index.Created = created;
        index.Hash = hash;
        index.Status = status;

        data.Add(index);

        return index;
    }

    public int Count()
    {
        return data.Count;
    }

    /// <summary>
    /// initialization resets the ledger data if existing data is called.
    /// </summary>
    public void Initialize()
    {
        // TODO since index reader/writer is injectable, we should make this call directly
        if (true == File.Exists(IndexFile))
            File.Delete(IndexFile);

        isLoaded = true;
    }

    public void InitializeFromSync(List<ILedgerIndex> rows)
    {
        if (true == isLoaded)
            throw new LedgerNotValidException("Ledger Index is already initialized");
        
        data.Clear();
        foreach(ILedgerIndex line in rows)
        {
            data.Add(line);
        }
        isLoaded = true;
        
        Save();
    }

    public void Load()
    {
        data = Reader.GetLedgerIndex((data) => {
            return LedgerIndex.FromString(data);
        });
    }

    /// <summary>
    /// TODO: this implementation is very inefficient
    /// </summary>
    public void Save()
    {
        Writer.SaveLedgerIndex(data);
    }

    public int GetNextBlockId()
    {
        ILedgerIndex mostrecent = data.OrderByDescending(u => u.BlockId).FirstOrDefault();
        return (mostrecent == null ? 0 : mostrecent.BlockId) + 1;
    }

    public ILedgerIndex GetIndex(int id)
    {
        return data.FirstOrDefault(e => e.BlockId == id);
    }

    public List<ILedgerIndex> ListAllIndexes()
    {
        return data.OrderBy(x => x.BlockId).ToList();
    }
}
