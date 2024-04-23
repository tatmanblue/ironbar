namespace core.Ledger
{
    public interface IPhysicalBlockValidator
    {
        /// <summary>
        /// Throws LedgerBlockException if block does not validate
        /// </summary>
        /// <param name="pb"></param>
        void Validate(ILedgerIndexManager index, ILedgerPhysicalBlock pb);
    }
    
}