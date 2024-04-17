using core.Ledger;
using Node.General;

namespace Node.Interfaces;

public interface IServicesEventPub
{
    void FireClientConnected(ChildNodeConnection cn);
    void FireServiceShutdown(ChildNodeConnection cn);
    void FireBlockCreated(ILedgerPhysicalBlock pb);
    void FireIndexInitialized();
}