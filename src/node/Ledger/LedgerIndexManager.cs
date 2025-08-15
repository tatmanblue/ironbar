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
    private readonly ILedgerWriter writer;
    private readonly ILedgerReader reader;
    private readonly ILedgerIndexFactory indexFactory;
    private List<ILedgerIndex> data = new List<ILedgerIndex>();
    private bool isLoaded = false;
    private string ledgerName = string.Empty;

    public LedgerIndexManager(string name, ILedgerReader reader, ILedgerWriter writer, ILedgerIndexFactory indexFactory)
    {
        ledgerName = name;
        this.writer = writer;
        this.reader =  reader;
        this.indexFactory = indexFactory;
    }

    public ILedgerIndex Add(string hash, DateTime created, BlockStatus status)
    {
        if (false == isLoaded)
            throw new LedgerException(ledgerName, "Attempted to add index before loading");
        
        ILedgerIndex index = indexFactory.Create(GetNextBlockId(), hash, created, status);
        data.Add(index);

        return index;
    }

    public int Count()
    {
        return data.Count;
    }

    /// <summary>
    /// initialization starts a new index file
    /// </summary>
    public void Initialize()
    {
        data = new List<ILedgerIndex>();
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

    public void Validate()
    {
        if (false == isLoaded)
            throw new LedgerNotValidException("Ledger Index is not loaded");
    }

    public void Load()
    {
        data = reader.GetLedgerIndex((data) => indexFactory.Create(data));
        isLoaded = true;
    }

    /// <summary>
    /// TODO: this implementation is very inefficient
    /// </summary>
    public void Save()
    {
        writer.SaveLedgerIndex(data);
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
