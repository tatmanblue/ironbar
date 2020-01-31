using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using core.Ledger;

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

        public override string ToString()
        {
            return $"{BlockID}:{Hash}:{Created}";
        }
    }

    /// <summary>
    /// Ledger index is a simplified collection of data about
    /// ledger blocks (currently physical blocks).  The manager
    /// provides the functionality for accessing the data
    /// </summary>
    public class LedgerIndexManager
    {
        private List<LedgerIndex> data = new List<LedgerIndex>();
        private bool isLoaded = false;

        public string IndexFile { get; private set; }
        public string LedgerName { get; private set; }

        public LedgerIndexManager(string indexFile, string name)
        {
            IndexFile = indexFile;
            LedgerName = name;
        }

        public LedgerIndex Add(string hash, DateTime created)
        {
            if (false == isLoaded)
                throw new LedgerException(LedgerName, "Attempted to add index before loading");

            LedgerIndex index = new LedgerIndex();
            index.BlockID = GetNextBlockId();
            index.Created = created;
            index.Hash = hash;

            data.Add(index);

            return index;
        }

        public void Initialize()
        {
            // TODO: blow away any file that exists!
            isLoaded = true;
        }

        public void Load()
        {
            isLoaded = true;
        }

        public void Save()
        {
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
            LedgerIndex mostrecent = data.OrderByDescending(u => u.BlockID).FirstOrDefault();
            return (mostrecent == null ? 0 : mostrecent.BlockID) + 1 ;
        }
    }
}
