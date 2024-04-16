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
    private List<LedgerIndex> data = new List<LedgerIndex>();
    private bool isLoaded = false;

    public string IndexFile { get; private set; }
    public string LedgerName { get; private set; }

    public LedgerIndexManager(string name, string indexFile)
    {
        IndexFile = indexFile;
        LedgerName = name;
    }

    public LedgerIndex Add(string hash, DateTime created, BlockStatus status)
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
        if (true == File.Exists(IndexFile))
            File.Delete(IndexFile);

        isLoaded = true;
    }

    public void Load()
    {
        data.Clear();

        string[] lines = File.ReadAllLines(IndexFile);
        foreach(string line in lines)
        {
            data.Add(LedgerIndex.FromString(line));
        }
        isLoaded = true;
    }

    /// <summary>
    /// TODO: this implementation is very inefficient
    /// </summary>
    public void Save()
    {
        // because we write all lines, have to delete the existing file first
        if (true == File.Exists(IndexFile))
            File.Delete(IndexFile);
        
        using (StreamWriter sw = File.AppendText(IndexFile))
        {
            foreach (LedgerIndex idx in data)
            {
                sw.WriteLine(idx.ToString());
            }

            sw.Flush();
            sw.Close();
        }
    }

    public int GetNextBlockId()
    {
        LedgerIndex mostrecent = data.OrderByDescending(u => u.BlockId).FirstOrDefault();
        return (mostrecent == null ? 0 : mostrecent.BlockId) + 1;
    }

    public LedgerIndex GetIndex(int id)
    {
        return data.First(e => e.BlockId == id);
    }

    public List<LedgerIndex> ListAllIndexes()
    {
        return data.OrderBy(x => x.BlockId).ToList();
    }
}
