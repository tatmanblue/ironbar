using System;
using core.Utility;

namespace core.Ledger
{
    /// <summary>
    /// all blocks have to be signed.  that is part of the security to ensure
    /// blocks are not tampered with
    /// TODO:  Not sure why I made this an interface
    /// </summary>
    public interface ILedgerSignBlock
    {
        Nonce Nonce { get; }
        DateTime DateStamp { get; }
        // TODO: need signature
    }
}
