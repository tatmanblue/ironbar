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

        /// <summary>
        /// Very important:  a ledger instance has to be valid to be used
        /// each ledger knows what it means to be valid, but in general it
        /// means each transaction on the ledger matches criteria for being valid
        /// </summary>
        void Validate();
    }
}
