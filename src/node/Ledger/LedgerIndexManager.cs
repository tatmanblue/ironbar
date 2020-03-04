using core.Ledger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace node.Ledger
{
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

        public LedgerIndexManager(string name, string indexFile)
        {
            IndexFile = indexFile;
            LedgerName = name;
        }

        public LedgerIndex Add(string hash, DateTime created)
        {
            if (false == isLoaded)
                throw new LedgerException(LedgerName, "Attempted to add index before loading");

            LedgerIndex index = new LedgerIndex();
            index.BlockId = GetNextBlockId();
            index.Created = created;
            index.Hash = hash;

            data.Add(index);

            return index;
        }

        public int Count()
        {
            return data.Count;
        }

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

        public void Save()
        {
            // TODO:  we are writing all longs so AppendText is not a good call
            // eval and fix
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
    }
}
