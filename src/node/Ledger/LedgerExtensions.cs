using core.Ledger;
using core.Plugin;

namespace Node.Ledger;

public static class LedgerExtensions
{
    public static void StartLedger(this IApplicationBuilder app)
    {
        ILedgerManager manager = app.ApplicationServices.GetService(typeof(ILedgerManager)) as ILedgerManager;
        manager.Start(app.ApplicationServices);
    }
    
    public static void StopLedger(this IApplicationBuilder app)
    {
        ILedgerManager manager = app.ApplicationServices.GetService(typeof(ILedgerManager)) as ILedgerManager;
        manager.Stop();
    }
    
    public static void FindAndStartPlugins(this IApplicationBuilder app)
    {
        // IEnumerable<IPlugin> plugins;
        // ILedgerManager manager = app.ApplicationServices.GetService(typeof(ILedgerManager)) as ILedgerManager;
        // manager.AddPlugins(plugins);
    }
    
    public static void StopPlugins(this IApplicationBuilder app)
    {
        // IEnumerable<IPlugin> plugins;
        // ILedgerManager manager = app.ApplicationServices.GetService(typeof(ILedgerManager)) as ILedgerManager;
        // manager.RemovePlugins(plugins);
    }
}
