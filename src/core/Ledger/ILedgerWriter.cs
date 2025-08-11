using System;
using System.Collections.Generic;
using System.Text;

namespace core.Ledger
{
    /// <summary>
    /// ledger readers and writers are used to read and write blocks to persistent storage and that can
    /// vary based on the storage mechanism used (file system, database, etc).
    /// </summary>
    public interface ILedgerWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="block"></param>
        void SaveBlock(ILedgerPhysicalBlock block);
        void SaveLedgerIndex(List<ILedgerIndex> index);
    }
}
