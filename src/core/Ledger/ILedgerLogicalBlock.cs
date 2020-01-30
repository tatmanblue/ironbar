using System;
using System.Collections.Generic;
using core.Utility;

namespace core.Ledger
{

    /// <summary>
    /// This represents a block in memory 
    /// TODO: signing blocks
    /// </summary>
    public interface ILedgerLogicalBlock<T>
    {
        int LedgerId { get; }
        DateTime TimeStamp { get; }
        Nonce Nonce { get; }
        string PreviousHash { get; }
        string Hash { get; }
        List<T> TransactionData { get; }
        public BlockStatus Status { get; }

        string ComputeHash();
    }
}
