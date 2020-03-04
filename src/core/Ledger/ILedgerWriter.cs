using System;
using System.Collections.Generic;
using System.Text;

namespace core.Ledger
{
    public interface ILedgerWriter
    {
        void SaveBlock(string path, ILedgerPhysicalBlock block);
    }
}
