using System;
using System.Collections.Generic;
using System.Text;

namespace core.Ledger
{
    public class LedgerNotValidException : Exception
    {
        public LedgerNotValidException() : base() { }
        public LedgerNotValidException(string message) : base(message) { }
    }
}
