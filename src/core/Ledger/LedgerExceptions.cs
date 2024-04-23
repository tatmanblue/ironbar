using System;

namespace core.Ledger
{
    public class LedgerBlockException : Exception
    {
        public LedgerBlockException(string message) : base(message) {}
    }
    
    public class LedgerNotValidException : Exception
    {
        public LedgerNotValidException() : base() { }
        public LedgerNotValidException(string message) : base(message) { }
    }
    
    /// <summary>
    /// General Exeception for ledger implementations
    /// Prefer to have more specific exceptions and save this for "the exception"
    /// to that rule
    /// </summary>
    public class LedgerException : Exception
    {
        public string Name { get; private set; }

        public LedgerException(string name, Exception ex) : base(ex.Message, ex)
        {
            Name = name;
        }

        public LedgerException(string name, string message) : base(message)
        {
            Name = name;
        }
    }
    
    public class LedgerNotFoundException : Exception
    {
        public string Name { get; private set; }

        public LedgerNotFoundException(string name) : base($"{name} was not found")
        {
            Name = name;
        }
    }
}