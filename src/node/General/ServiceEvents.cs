using core.Ledger;

namespace Node.General;

public delegate void ChildNodeConnected(ChildNodeConnection cn);
public delegate void NotifyChildOfShutdown(ChildNodeConnection cn);
public delegate void BlockCreated(ILedgerPhysicalBlock pb);
public delegate void IndexInitialized();