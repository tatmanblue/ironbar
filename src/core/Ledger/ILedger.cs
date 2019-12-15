using System;
using System.Collections.Generic;
using System.Text;

namespace core.Ledger
{
    public interface ILedger
    {
        int Id { get; }
        string Path { get; }
        LedgerState State { get; }
    }
}
