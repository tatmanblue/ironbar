using System;
namespace core.Ledger
{
    public class LedgerNotFoundException : Exception
    {
        public string Name { get; private set; }

        public LedgerNotFoundException(string name) : base($"{name} was not found")
        {
            Name = name;
        }
    }
}
