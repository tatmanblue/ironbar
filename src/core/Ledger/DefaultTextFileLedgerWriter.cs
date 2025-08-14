using System;
using System.Collections.Generic;
using System.IO;

namespace core.Ledger
{
    public class DefaultTextFileLedgerWriter : ILedgerWriter
    {
        private string ledgerPath = string.Empty;
        private string ledgerIndexFileName => System.IO.Path.Combine(ledgerPath, "index.txt");
        public DefaultTextFileLedgerWriter(string path)
        {
            ledgerPath = path;
        }

        public void SaveBlock(ILedgerPhysicalBlock block)
        {
            string fileName = System.IO.Path.Combine(ledgerPath, $"block.{block.Id}.txt");
            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.Write(block.ToString());
                sw.Flush();
                sw.Close();
            }
        }
        
        public void SaveLedgerIndex(List<ILedgerIndex> data)
        {
            if (true == File.Exists(ledgerIndexFileName))
                File.Delete(ledgerIndexFileName);
        
            using (StreamWriter sw = File.AppendText(ledgerIndexFileName))
            {
                foreach (ILedgerIndex idx in data)
                {
                    sw.WriteLine(idx.ToString());
                }

                sw.Flush();
                sw.Close();
            }
        }

        public void InitializeStorage()
        {
            if (false == Directory.Exists(ledgerPath))
                Directory.CreateDirectory(ledgerPath);
        }

        public void CheckStorage()
        {
            // check that the directory exists
            if (false == Directory.Exists(ledgerPath))
                throw new LedgerNotFoundException($"Ledger directory not found {ledgerPath}");

            // check that the index file exists
            if (false == File.Exists(ledgerIndexFileName))
                throw new LedgerNotFoundException($"Ledger index file not found {ledgerIndexFileName}");
        }
    }
}
