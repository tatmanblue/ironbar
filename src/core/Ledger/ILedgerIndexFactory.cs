using System;

namespace core.Ledger;

/// <summary>
/// To abstract the creation of LedgerIndex instances with idea that different implementations can be provided.
/// </summary>
public interface ILedgerIndexFactory
{
    ILedgerIndex CreateLedgerIndex(int blockId, string hash, DateTime created, BlockStatus status);
    ILedgerIndex CreateLedgerIndex(string data);
}
