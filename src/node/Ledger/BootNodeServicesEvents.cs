using core.Ledger;
using Node.General;
using Node.Interfaces;

namespace Node.Ledger;

#region boot node ledger events
/// <summary>
/// 
/// </summary>
public class BootNodeServicesEvents : IServicesEventPub, IServicesEventSub
{
    public virtual void FireBlockCreated(ILedgerPhysicalBlock pb)
    {
        BlockCreated bc = OnBlockCreated;
        if (null == bc) return;
        bc(pb);
    }

    public virtual void FireClientConnected(ChildNodeConnection connection)
    {
        ChildNodeConnected nc = OnChildNodeConnected;
        if (null == nc) return;
        nc(connection);
    }

    #region ILedgerEventSub
    public event BlockCreated OnBlockCreated;
    public event ChildNodeConnected OnChildNodeConnected;
    #endregion
}
#endregion

#region Child node ledger events
/// <summary>
/// TODO this is really ugly.  Need to find better way than having two implementations
/// TODO one for boot and one for child nodes
/// </summary>
public class ClientNodeServicesEvents : BootNodeServicesEvents
{
    public override void FireBlockCreated(ILedgerPhysicalBlock pb)
    {
    }
    
    public override void FireClientConnected(ChildNodeConnection connection)
    {
    }
}

#endregion