using System;
using Microsoft.AspNetCore.Builder;
using Node.Interfaces;

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
}
