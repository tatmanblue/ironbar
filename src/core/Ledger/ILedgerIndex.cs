using System;
namespace core.Ledger
{
    public interface ILedgerIndex
    {
        int BlockId { get; }
        string Hash { get; }
        /// <summary>
        /// Reflection of ILedgerPhysicalBlock.Status.  Any differences in opinion revert
        /// to ILedgerPhysicalBlock.Status
        /// </summary>
        BlockStatus Status { get; }
        DateTime Created { get; }
    }
}
