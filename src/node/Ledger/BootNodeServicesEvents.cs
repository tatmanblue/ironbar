﻿using core.Ledger;
using Node.General;
using Node.Interfaces;

namespace Node.Ledger;

#region boot node ledger events
/// <summary>
/// 
/// </summary>
public class BootNodeServicesEvents : IServicesEventPub, IServicesEventSub
{
    private ILogger<BootNodeServicesEvents> logger;
    
    public async virtual void FireBlockCreated(ILedgerPhysicalBlock pb)
    {
        BlockCreated bc = OnBlockCreated;
        if (null == bc) return;
        await Task.Run(() => {
            bc.Invoke(pb);
        });
    }

    public virtual void FireClientConnected(ChildNodeConnection connection)
    {
        ChildNodeConnected nc = OnChildNodeConnected;
        if (null == nc) return;
        
        logger?.LogDebug("Invoking OnChildNodeConnected");
        nc.Invoke(connection);
    }

    public virtual void FireServiceShutdown(ChildNodeConnection cn)
    {
        NotifyChildOfShutdown shutdown = OnShutdown;
        if (null == shutdown) return;
        shutdown.Invoke(cn);
    }

    public virtual void FireIndexInitialized()
    {
        IndexInitialized indexInitialized = OnIndexInitialized;
        if (null == indexInitialized) return;
        indexInitialized.Invoke();
    }

    public BootNodeServicesEvents(IServiceProvider serviceProvider)
    {
        this.logger = serviceProvider.GetRequiredService<ILogger<BootNodeServicesEvents>>();
    }

    #region ILedgerEventSub
    public event BlockCreated OnBlockCreated;
    public event ChildNodeConnected OnChildNodeConnected;
    public event IndexInitialized OnIndexInitialized;
    public event NotifyChildOfShutdown OnShutdown;
    #endregion
}
#endregion

#region Child node ledger events
/// <summary>
/// TODO this is really ugly.  Need to find better way than having two implementations
/// TODO one for boot and one for child nodes
/// </summary>
public class ChildNodeServicesEvents : BootNodeServicesEvents
{
    public override void FireBlockCreated(ILedgerPhysicalBlock pb)
    {
    }
    
    public override void FireClientConnected(ChildNodeConnection connection)
    {
    }
    
    public override void FireServiceShutdown(ChildNodeConnection cn)
    {
    }

    public ChildNodeServicesEvents(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        
    }
}

#endregion