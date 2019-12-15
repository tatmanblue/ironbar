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
        int LedgerId { get; }
        DateTime TimeStamp { get; }
        long Nonce { get; }
        string PreviousHash { get; }
        string Hash { get; }
        byte[] TransactionData { get; }
        public BlockStatus Status { get; }
        string ComputeHash();
    }
}
