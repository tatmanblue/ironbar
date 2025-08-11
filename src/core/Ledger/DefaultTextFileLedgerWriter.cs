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
    }
}
