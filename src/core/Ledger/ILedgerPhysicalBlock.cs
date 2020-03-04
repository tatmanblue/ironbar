using System;
using core.Utility;

namespace core.Ledger
{
    /// <summary>
    /// this represents data as it will appear when saved on disk
    /// TODO: signing blocks
    /// </summary>
    public interface ILedgerPhysicalBlock
    {
        /// <summary>
        /// ID of THIS block
        /// </summary>
        int Id { get; }
        /// <summary>
        /// ID of parent block, should be sequential thus ID - 1
        /// </summary>
        int ParentId { get; }
        /// <summary>
        /// ID of block that this block has a reference to.  Reference
        /// being a change in the block data. Does not have to be sequential
        /// </summary>
        int ReferenceId { get; }
        string ParentHash { get; }
        int LedgerId { get; }
        DateTime TimeStamp { get; }
        Nonce Nonce { get; }
        string Hash { get; }
        byte[] TransactionData { get; }
        BlockStatus Status { get; }
        ILedgerSignBlock SignBlock { get; }

        string ComputeHash();
    }
}
