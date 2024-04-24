using System;
namespace core
{
    /// <summary>
    /// 
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// this is the friendly name that appears in logs etc....
        /// </summary>
        string FriendlyName { get; }
        /// <summary>
        /// true means the node is bootnode, which means the node controls
        /// behaviors of other nodes. 
        /// </summary>
        bool IsBootNode { get; }
        /// <summary>
        /// The node, either as boot or child, will listen for RPC calls on this port
        /// </summary>
        int RPCPort { get; }
        /// <summary>
        /// Only important when IsBootNode == false, indicates how to locate the
        /// boot node
        /// </summary>
        string BootAddress { get; }
        /// <summary>
        /// path to plugins
        /// </summary>
        string PluginPath { get; }
        /// <summary>
        /// where all of the blockchain stuff goes
        /// </summary>
        string DataPath { get; }      
    }
}
