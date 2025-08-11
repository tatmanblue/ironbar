using System;
using System.Collections.Generic;
using System.Text;

namespace core.Ledger
{
    /// <summary>
    /// ledger readers and writers are used to read and write blocks to persistent storage and that can
    /// vary based on the storage mechanism used (file system, database, etc).
    /// </summary>
    public interface ILedgerReader
    {
        /// <summary>
        /// returns the number of blocks found in the ledger
        /// </summary>
        /// <returns></returns>
        int CountBlocks();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ILedgerPhysicalBlock GetLedgerPhysicalBlock(int id, Func<string, ILedgerPhysicalBlock> blockAllocator);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="indexAllocator"></param>
        /// <returns></returns>
        ILedgerIndex GetLedgerIndex(int index, Func<string, ILedgerIndex> indexAllocator);
        /// <summary>
        /// loads the entire ledger index into memory
        /// </summary>
        /// <param name="index"></param>
        /// <param name="indexAllocator"></param>
        /// <returns></returns>
        List<ILedgerIndex> GetLedgerIndex(Func<string, ILedgerIndex> indexAllocator);
    }
}
