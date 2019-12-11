using System;
using System.Collections.Generic;

namespace Core.Plugin
{
    // all plugins must implement this interface
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
