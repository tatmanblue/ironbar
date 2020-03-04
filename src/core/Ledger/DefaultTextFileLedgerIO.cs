using System;
using System.IO;

namespace core.Ledger
{
    public class DefaultTextFileLedgerIO : ILedgerWriter
    {
        public void SaveBlock(string path, ILedgerPhysicalBlock block)
        {
            string fileName = System.IO.Path.Combine(path, $"block.{block.Id}.txt");
            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.Write(block.ToString());
                sw.Flush();
                sw.Close();
            }
        }
    }
}
