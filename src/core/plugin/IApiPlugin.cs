using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Plugin
{
    /// <summary>
    /// plugins that provide a API to be consumed (aka RESTFul service etc)
    /// must implement this plugin
    /// </summary>
    public interface IApiPlugin : IPlugin
    {
    }
}
