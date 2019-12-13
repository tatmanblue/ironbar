using System;
using Microsoft.AspNetCore.Builder;

namespace node.Ledger
{
    public static class LedgerExtensions
    {
        public static void StartLedger(this IApplicationBuilder app)
        {
            ILedgerManager manager = app.ApplicationServices.GetService(typeof(ILedgerManager)) as ILedgerManager;
            manager.Start();
        }
    }
}
