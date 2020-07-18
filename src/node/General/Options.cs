using System;
using core;

namespace node.General
{
    public class Options : IConfiguration
    {
        /// <summary>
        /// this is the friendly name that appears in logs etc....
        /// </summary>
        public string FriendlyName { get; set; }
        public int RPCPort { get; set; } = 5001;
        public int ServerRPCPort { get; set; } = 5001;
        public int APIPort { get; set; } = 8080;
        public string PluginPath { get; set; } = "plugins";
        public string DataPath { get; set; } = "data";

        /// <summary>
        /// when RPCPort and ServerRPCPort are the same, the node assumes its is the 
        /// bootnode
        /// </summary>
        public bool IsBootNode
        {
            get
            {
                return RPCPort == ServerRPCPort;
            }
        }
    }
}
