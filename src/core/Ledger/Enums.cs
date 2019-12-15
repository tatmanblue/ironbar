using System;
using System.Collections.Generic;
using System.Text;

namespace core.Ledger
{
    /// <summary>
    /// State of block.  When a block is created it is unconfirmed.  Going through the consensus
    /// functionality the block will go to confirmed to approved.  Confirmed means nodes accept
    /// the block and approved means consensus is complete.
    /// </summary>
    public enum BlockStatus
    {
        Unconfirmed,
        Confirmed,
        Approved,
        Rejected
    }

    public enum LedgerState
    {
        Nonfunctional,
        StartingUp,
        Available,
        ShuttingDown,
    }
}
