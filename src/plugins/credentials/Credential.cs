using System;
using System.Collections.Generic;
using Core.Plugin;

public class Credenital : IPlugin
{
    private static readonly string VERSION = "0.0.0.1";
    private static readonly string ID = "IronBar Credentials";
    public void Init()
    {
    }

    public string Version()
    {
        return VERSION;
    }

    public string Id()
    {
        return ID;
    }
}
