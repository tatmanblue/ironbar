using System;
namespace node.Ledger
{
    [Serializable]
    public class LedgerIndex
    {
        /// <summary>
        /// Ledger.Id
        /// </summary>
        public int BlockID { get; internal set; }
        public string Hash { get; internal set; }
        public DateTime Created { get; internal set; }
    }

    /// <summary>
    /// Ledger index is a simplified collection of data about
    /// ledger blocks (currently physical blocks).  The manager
    /// provides the functionality for accessing the data
    /// </summary>
    public class LedgerIndexManager
    {
        public string IndexFile { get; private set; }

        public LedgerIndexManager(string ledgerPath)
        {

        }

        public LedgerIndex Add(string hash, DateTime created)
        {
            LedgerIndex index = new LedgerIndex();
            index.BlockID = 0; // TODO!!!
            index.Created = created;
            index.Hash = hash;

            Save();
            return index;
        }

        public void Load()
        {

        }

        public void Save()
        {

        }
    }
}
