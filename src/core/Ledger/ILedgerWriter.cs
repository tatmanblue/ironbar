using System;
using System.Collections.Generic;
using System.Text;

namespace core.Ledger
{
    public interface ILedgerWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="block"></param>
        void SaveBlock(ILedgerPhysicalBlock block);
    }
}
