using System;
namespace core.Ledger
{
    public class LedgerException : Exception
    {
        public string Name { get; private set; }

        public LedgerException(string name, string message) : base(message)
        {
            Name = name;
        }
    }
}
