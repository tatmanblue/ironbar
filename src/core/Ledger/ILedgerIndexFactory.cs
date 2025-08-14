using System;

namespace core.Ledger;

/// <summary>
/// To abstract the creation of LedgerIndex instances with idea that different implementations can be provided.
/// </summary>
public interface ILedgerIndexFactory
{
    ILedgerIndex Create(int blockId, string hash, DateTime created, BlockStatus status);
    /// <summary>
    /// Primarily for creating new index records via "serialized" data
    /// </summary>
    /// <param name="data"></param>
    /// <returns>ILedgerIndex</returns>
    ILedgerIndex Create(string data);
}
