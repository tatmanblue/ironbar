using System;
using System.Collections.Generic;

namespace core.Ledger
{

    /// <summary>
    /// At some point, there might be several ledger types and each
    /// manager is responsible for one type.
    /// </summary>
    public interface ILedgerManager
    {
        void Start(IServiceProvider serviceProvider);
        void Stop();

        /// <summary>
        /// for creating a new block into the system.  system generates the entire block
        /// from the input which is the data the consumer wants saved in the block
        /// </summary>
        /// <param name="blockData">consumer data to be part of the block</param>
        /// <returns></returns>
        ILedgerPhysicalBlock Create(string blockData);

        /// <summary>
        /// "Advances" a block state.  Such as to confirmed from unconfirmed
        /// Block data does not change.  Block reference Id references previous block
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        ILedgerPhysicalBlock AdvanceBlock(ILedgerPhysicalBlock pb, BlockStatus status);
        ILedgerPhysicalBlock GetBlock(int id);
        List<ILedgerIndex> ListAllBlocks();

        string CreateIndexesVerification(IList<string> rows);
        /// <summary>
        /// used by child nodes to get in sync with bootnoode
        /// </summary>
        /// <param name="rows"></param>
        void SyncIndex(IList<string> rows, string verification);

        /// <summary>
        /// a child node only function for adding a block to local ledger
        /// </summary>
        /// <param name="block"></param>
        /// <param name="verification"></param>
        /// <returns></returns>
        ILedgerPhysicalBlock SyncBlock(string block, string verification);
    }
}