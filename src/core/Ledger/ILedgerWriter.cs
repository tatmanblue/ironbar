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
        /// <summary>
        /// Ensures the storage mechanism is ready for use
        /// For boot nodes, typically this means creating the storage if it does not exist
        /// Always called for child nodes, even if the storage already exists
        /// </summary>
        void InitializeStorage();
        /// <summary>
        /// Verifies that the storage is available and writable
        /// Implementations should not initialize or create storage here as this such errors are
        /// meant to trigger initialization workflow
        /// <exception cref="LedgerNotFoundException">Thrown when ledger data expected is not found</exception>
        /// </summary>
        void CheckStorage();
    }
}
