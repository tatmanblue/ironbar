using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace core.Ledger
{
    public class DefaultTextFileLedgerReader : ILedgerReader
    {
        private string ledgerPath = string.Empty;
        private string ledgerIndexFileName => System.IO.Path.Combine(ledgerPath, "index.txt");
        public DefaultTextFileLedgerReader(string path)
        {
            ledgerPath = path;
        }

        public int CountBlocks()
        {
            try
            {
                // TODO:  this can take a long time
                // https://docs.microsoft.com/en-us/dotnet/api/system.io.directory.getfiles?view=netframework-4.8#System_IO_Directory_GetFiles_System_String_System_String_
                // probably optimize this in some way
                return System.IO.Directory.GetFiles(ledgerPath, $"block.*.txt").Length;
            } catch (IOException)
            {
                throw new LedgerNotFoundException("No ledger directory found");
            }
        }

        public ILedgerPhysicalBlock GetLedgerPhysicalBlock(int id, Func<string, ILedgerPhysicalBlock> blockAllocator)
        {
            string blockData = System.IO.File.ReadAllText(System.IO.Path.Combine(ledgerPath, $"block.{id}.txt"));
            return blockAllocator(blockData);
        }
        
        public ILedgerIndex GetLedgerIndex(int index, Func<string, ILedgerIndex> indexAllocator)
        {
            string[] lines = File.ReadAllLines(ledgerIndexFileName);
            if (lines.Length > 0 && index >= 0 && index < lines.Length)
                return indexAllocator(lines[index]);

            throw new LedgerNotValidException("No ledger index found");
        }
        
        public List<ILedgerIndex> GetLedgerIndex( Func<string, ILedgerIndex> indexAllocator)
        {
            try
            {
                string[] lines = File.ReadAllLines(ledgerIndexFileName);
                List<ILedgerIndex> result = new List<ILedgerIndex>();
                if (lines.Length > 0)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        result.Add(indexAllocator(lines[i]));
                    }

                    return result;
                }
                // TODO this exception is not quite right possibly
                throw new LedgerNotValidException("No ledger index found");
            }
            catch (IOException e)
            {
                throw new LedgerNotValidException("No ledger index found");
            }
            
        }
    }
}
