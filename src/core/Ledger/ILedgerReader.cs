using System;
using System.Collections.Generic;
using System.Text;

namespace core.Ledger
{
    public interface ILedgerReader
    {
        /// <summary>
        /// returns the number of blocks found in the ledger
        /// </summary>
        /// <returns></returns>
        int CountBlocks();
    }
}
