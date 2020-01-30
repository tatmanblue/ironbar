using System;
using System.Collections.Generic;
using System.Text;

namespace core.Ledger
{
    /// <summary>
    /// this represents data as it will appear when saved on disk
    /// TODO: signing blocks
    /// </summary>
    public interface ILedgerPhysicalBlock
    {
        int ParentId { get; }
        string ParentHash { get; }
        int LedgerId { get; }
        DateTime TimeStamp { get; }
        long Nonce { get; }
        string Hash { get; }
        byte[] TransactionData { get; }
        public BlockStatus Status { get; }
        string ComputeHash();
    }
}
