using System;

namespace core.Ledger
{
    public interface ILedgerIndexFactory
    {
        ILedgerIndex CreateLedgerIndex(int blockId, string hash, DateTime created, BlockStatus status);
        ILedgerIndex CreateLedgerIndex(string data);
    }
}