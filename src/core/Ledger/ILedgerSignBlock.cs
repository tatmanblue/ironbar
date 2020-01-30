using System;
using core.Utility;

namespace core.Ledger
{
    public interface ILedgerSignBlock
    {
        Nonce Nonce { get; }
        DateTime DateStamp { get; }
    }
}
