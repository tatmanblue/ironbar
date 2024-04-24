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
        Error,                  
        Unconfirmed,                                // starting state for any block
        Confirmed,                                  // sent by child node stating unconfirmed block is acceptable
                                                    // if a block is saved as confirmed, it means theres no child nodes
        Approved,                                   // updated by boot node when unconfirmed nodes confirm block
        System,                                     // mainly for the index block
        Rejected,                                   // updated by boot node when enough nodes deny block
        SystemDeleted                               // special case where status was set without adding new block
    }

    public enum LedgerState
    {
        Nonfunctional,
        StartingUp,
        Available,
        ShuttingDown,
    }
}
