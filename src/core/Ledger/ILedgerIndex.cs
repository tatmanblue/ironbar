using System;
namespace core.Ledger
{
    public interface ILedgerIndex
    {
        int BlockId { get; }
        string Hash { get; }
        DateTime Created { get; }
    }
}
