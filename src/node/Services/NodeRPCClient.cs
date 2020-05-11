using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grpc.Net.Client;
using node.General;

namespace node
{
    /// <summary>
    /// this is currently used to use GRPC generated code for a child node to talk
    /// to the bootnode.  It is used by ClientNodeService
    /// TODO: not sure but maybe this should be consolidated into ClientNodeService
    /// TODO: interface so it can be mocked/injected
    /// </summary>
    public class NodeRPCClient
    {
        private readonly ILogger<NodeRPCClient> _logger;
        private readonly IOptions _options;
        public NodeRPCClient(IOptions options, ILogger<NodeRPCClient> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task<bool> ConnectToBootNode(int delay = 2000)
        {
            {
                try
                {
                    Task.Delay(delay).Wait();
                    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                    string localIP = $"{LocalIPAddress()}";
                    // TODO: bootNode may not be on the same host, and may be using https
                    string bootNodeIP = $"http://localhost:{_options.ServerRPCPort}";
                    _logger.LogInformation($"Attempting connect to channel is: {bootNodeIP} and my ip is {localIP}");
                    var channel = GrpcChannel.ForAddress(bootNodeIP);
                    var client = new BootNode.BootNodeClient(channel);
                    var reply = client.AddLink(new LinkRequest { ClientAddr = $"http://{localIP}:{_options.RPCPort}" });
                    _logger.LogInformation("BootNode says: " + reply.Message);

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"ERROR: {ex.Message}");
                    if (ex.InnerException != null)
                        _logger.LogInformation($"INNER EXCEPTION: {ex.InnerException}");
                    else
                        _logger.LogInformation("no more details");

                    return false;
                }
            }
        }

        private IPAddress LocalIPAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            return host
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }
    }
}
