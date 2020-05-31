using System;
using System.Collections.Generic;

namespace core.Plugin
{
    /// <summary>
    /// Plugins must implement this interface but should not implement this interface directly
    /// as it is inherited from IApiPlugin or ILedgerPlugin
    /// </summary>
    public interface IPlugin
    {
        void Init();
        // must be in the format of "major.minor.patch.build"
        // to be accepted in release builds nothing starting with "0" will be allowed
        string Version();
        // must be an id that can be referenced by consumers.
        // TODO: not sure yet what to do in the event of id collisions
        string Id();
    }
}
