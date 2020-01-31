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
        int Id { get; }
        int ParentId { get; }
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
