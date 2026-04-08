using System;
using core.Utility;

namespace core.Ledger
{
    /// <summary>
    /// this represents data as it will appear when saved on disk
    /// TODO:  Not sure why I made this an interface
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
        /// ID of a block earlier in the chain whose data this block supersedes or advances.
        /// Used in the BFT confirmation flow: the Confirmed block produced by AdvanceBlock points
        /// back to the Unconfirmed block it is confirming.  A value of 0 means this block has no
        /// reference (it is an original submission).  ReferenceHash must match the hash of the
        /// referenced block; the validator enforces this when ReferenceId &gt; 0.
        /// </summary>
        int ReferenceId { get; }
        string ParentHash { get; }
        string ReferenceHash { get; }
        int LedgerId { get; }
        DateTime TimeStamp { get; }
        Nonce Nonce { get; }
        string Hash { get; }
        byte[] TransactionData { get; }
        BlockStatus Status { get; }
        ILedgerSignBlock SignBlock { get; }

        /// <summary>
        /// For serialization to JSON used by blob storage and other mechanisms
        /// (Notes ToString() overrides will be used by TextFileLedgerWriter)
        /// </summary>
        /// <returns></returns>
        string ToJson();
    }
}
