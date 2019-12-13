using System;
using System.Collections.Generic;

namespace core.Ledger
{
    /// <summary>
    /// this represents data as it will appear when saved on disk
    /// </summary>
    public interface ILedgerPhysicalBlock
    {
        int LedgerId { get; }
        DateTime TimeStamp { get; }
        long Nonce { get; }
        string PreviousHash { get; }
        string Hash { get; }
        byte[] TransactionData { get; }

        string ComputeHash();
    }

    /// <summary>
    /// This represents a block in memory 
    /// </summary>
    public interface ILedgerLogicalBlock<T>
    {
        int LedgerId { get; }
        DateTime TimeStamp { get; }
        long Nonce { get; }
        string PreviousHash { get; }
        string Hash { get; }
        List<T> TransactionData { get; }

        string ComputeHash();
    }
}
