using core.Ledger;

namespace Node.General;

public delegate void ChildNodeConnected(ChildNodeConnection cn);
public delegate void BlockCreated(ILedgerPhysicalBlock pb);