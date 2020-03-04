using System;
using System.IO;

namespace core.Ledger
{
    public class DefaultTextFileLedgerWriter : ILedgerWriter
    {
        private string ledgerPath = string.Empty;
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
    }
}
