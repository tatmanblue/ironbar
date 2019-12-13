using CommandLine;

namespace node.General
{
    public interface IOptions
    {
        /// <summary>
        /// true means the node is bootnode, which means the node controls
        /// behaviors of other nodes
        /// </summary>
        bool IsBootNode { get; }
        /// <summary>
        /// port for grpc calls when node is not bootnode
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

    public class Options : IOptions
    {
        [Option('r', HelpText = "Port to listen for RPC calls")]
        public int RPCPort { get; set; } = 5001;
        [Option('s', HelpText = "Server RPC call port")]
        public int ServerRPCPort { get; set; } = 5001;
        [Option('a', HelpText = "WebAPI port")]
        public int APIPort { get; set; } = 8080;
        [Option('p', HelpText = "plug-ins path")]
        public string PluginPath { get; set; } = "plugins";
        [Option('d', HelpText = "where the ledger data lives")]
        public string DataPath { get; set; } = "data";

        public bool IsBootNode
        {
            get
            {
                return RPCPort == ServerRPCPort;
            }
        }
    }
}
