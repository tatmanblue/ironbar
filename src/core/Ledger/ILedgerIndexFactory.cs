using System;
using System.Collections.Generic;

namespace core.Ledger;

/// <summary>
/// To abstract the creation of LedgerIndex instances with idea that different implementations can be provided.
/// </summary>
public interface ILedgerIndexFactory
{
    ILedgerIndex Create(int blockId, string hash, DateTime created, BlockStatus status);
    /// <summary>
    /// <summary>
    /// Primarily for creating new blocks via "serialized" data and isolating the implementation details
    /// </summary>
    /// </summary>
    /// <param name="data"></param>
    /// <returns>ILedgerIndex</returns>
    ILedgerIndex Create(string data);

    /// <summary>
    /// used for serialization only
    /// </summary>
    /// <returns>ILedgerIndex</returns>
    ILedgerIndex Create();
    
    List<ILedgerIndex> CreateList(string data);
}
