using System;
namespace core
{
    /// <summary>
    /// This replaces IOption (in node)
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// true means the node is bootnode, which means the node controls
        /// behaviors of other nodes. 
        /// </summary>
        bool IsBootNode { get; }
        /// <summary>
        /// port for child nodes to listen for grpc calls from bootnode (when node is not bootnode)
        /// unused when node is bootnode (it should be the same as ServerRPCPort)
        /// </summary>
        int RPCPort { get; }
        /// <summary>
        /// port for grpc calls to bootnode.  when node is bootnode, it listens
        /// on this port
        /// </summary>
        int ServerRPCPort { get; }
        /// <summary>
        /// port for restful calls
        /// </summary>
        int APIPort { get; }
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
