using core.Ledger;
using Node.General;

namespace Node.Interfaces;

public interface IServicesEventPub
{
    void FireClientConnected(ChildNodeConnection cn);
    void FireBlockCreated(ILedgerPhysicalBlock pb);
}