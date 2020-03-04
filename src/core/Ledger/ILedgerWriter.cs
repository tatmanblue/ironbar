using System;
using System.Collections.Generic;
using System.Text;

namespace core.Ledger
{
    public interface ILedgerWriter
    {
        /// <summary>
        /// TODO: remove path.  That should be part of type initialization. that allows more flexibility for
        /// ledger to be saved to different mechanisms
        /// </summary>
        /// <param name="path"></param>
        /// <param name="block"></param>
        void SaveBlock(string path, ILedgerPhysicalBlock block);
    }
}
