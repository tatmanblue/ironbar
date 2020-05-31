using System;
using System.Collections.Generic;
using core.Plugin;

namespace Credentials
{
    public class Credenital : ILedgerPlugin
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
}
