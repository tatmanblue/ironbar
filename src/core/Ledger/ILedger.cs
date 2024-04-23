using System;
using System.Collections.Generic;
using System.Text;

namespace core.Ledger
{
    public interface ILedger
    {
        int Id { get; }
        /// <summary>
        /// To support having multiple ledgers run by the same typology, each ledger
        /// is differentiated by a name (which also becomes the directory the ledger
        /// is written)
        /// </summary>
        string Name { get; }
        /// <summary>
        /// root directory/folder where the ledgers are saved.
        /// Ledgers will postfix their directory name to this path
        /// and save their work there.  Only applicable when ledgers
        /// are a file based system
        /// </summary>
        string RootDataPath { get; }
        LedgerState State { get; }

        // TODO: NOT Entirely sure ledger should know this.  It might add an additional
        // layer of security if the reader and writers are accessed by some other
        // means
        ILedgerWriter Writer { get; }
        ILedgerReader Reader { get; }
        
        /// <summary>
        /// Very important:  a ledger instance has to be valid to be used
        /// each ledger knows what it means to be valid, but in general it
        /// means each transaction on the ledger matches criteria for being valid
        /// </summary>
        void Validate();

        /// <summary>
        /// This creates a whole new ledger.  Danger! assume if the ledger already exists
        /// it gets over written
        /// </summary>
        void Initialize();

        /// <summary>
        /// part of initialization, breaking out initialize into parts.  Create the blank
        /// storage structure (eg for a file system implementation, the directories)
        /// </summary>
        void InitializeStorage();

        /// <summary>
        /// Expectation is this block is being added in which it already exists such
        /// as from an existing block on another node (aka boot node -> child node) 
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        ILedgerPhysicalBlock AddBlock(ILedgerPhysicalBlock block);
        /// <summary>
        /// Expectation is this block is being sourced from an external client
        /// such as a webpage or mobile app and so the block will be treated as new.  This
        /// implementation is expected to complete the block data before saving it
        /// </summary>
        /// <param name="data"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        ILedgerPhysicalBlock AddBlock(byte[] data, BlockStatus status = BlockStatus.Unconfirmed);
        ILedgerPhysicalBlock SyncBlock(ILedgerPhysicalBlock block);
        /// <summary>
        /// Throws LedgerBlockException if block fails to validate
        /// </summary>
        /// <param name="block"></param>
        void ValidateBlock(ILedgerPhysicalBlock block);
    }
}
