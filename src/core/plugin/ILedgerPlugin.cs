using System;
using System.Collections.Generic;
using System.Text;

namespace core.Plugin
{
    /// <summary>
    /// Plugins that provide a blockchain ledger must implement this plugin
    ///
    /// TODO this interface is not yet finalized and may change in future releases
    /// </summary>
    public interface ILedgerPlugin : IPlugin
    {
    }
}
