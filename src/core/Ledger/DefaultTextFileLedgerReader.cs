using System;
using System.Collections.Generic;
using System.Text;

namespace core.Ledger
{
    public class DefaultTextFileLedgerReader : ILedgerReader
    {
        private string ledgerPath = string.Empty;
        public DefaultTextFileLedgerReader(string path)
        {
            ledgerPath = path;
        }

        public int CountBlocks()
        {
            // TODO:  this can take a long time
            // https://docs.microsoft.com/en-us/dotnet/api/system.io.directory.getfiles?view=netframework-4.8#System_IO_Directory_GetFiles_System_String_System_String_
            // probably optimize this in some way
            return System.IO.Directory.GetFiles(ledgerPath, $"block.*.txt").Length;
        }

        public ILedgerPhysicalBlock GetLedgerPhysicalBlock(int id, Func<string, ILedgerPhysicalBlock> blockAllocator)
        {
            string blockData = System.IO.File.ReadAllText(System.IO.Path.Combine(ledgerPath, $"block.{id}.txt"));
            return blockAllocator(blockData);
        }
    }
}
