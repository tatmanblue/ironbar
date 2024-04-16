using System.Net;
using System.Net.Sockets;
using core.Utility;
using Grpc.Net.Client;
using IConfiguration = core.IConfiguration;

namespace Node.grpc.service; 

/// <summary>
/// this is currently used to use GRPC generated code for a child node to talk
/// to the bootnode.  It is used by ClientNodeService
/// TODO: not sure but maybe this should be consolidated into ClientNodeService
/// TODO: interface so it can be mocked/injected
/// </summary>
public class ChildNodeRPCClient
{
    private readonly ILogger<ChildNodeRPCClient> logger;
    private readonly IConfiguration options;
    public ChildNodeRPCClient(IConfiguration options, ILogger<ChildNodeRPCClient> logger)
    {
        this.options = options;
        this.logger = logger;
    }

    public async Task<bool> ConnectToBootNode(int delay = 2000)
    {
        try
        {
            Task.Delay(delay).Wait();
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            // TODO need to fix this and get actual address
            string localIP = "localhost";
            string bootNodeIP = $"{options.BootAddress}";
            logger.LogInformation($"Attempting connect to channel is: {bootNodeIP} and my ip is {IpAddressUtility.LocalIP().ToString()}");
            var channel = GrpcChannel.ForAddress(bootNodeIP);
            var client = new NodeToNodeConnection.NodeToNodeConnectionClient(channel);
            var reply = client.Connect(new ConnectRequest() { ClientAddr = $"http://{localIP}:{options.RPCPort}" });
            logger.LogInformation("BootNode says: " + reply.Message);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogInformation($"ERROR: {ex.Message}");
            if (ex.InnerException != null)
                logger.LogInformation($"INNER EXCEPTION: {ex.InnerException}");
            else
                logger.LogInformation("no more details");

            return false;
        }
    }

    public async Task<bool> SendShuttingDownMessage()
    {
        try
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            string localIP = $"{IpAddressUtility.LocalIP()}";
            string bootNodeIP = $"{options.BootAddress}";
            logger.LogInformation($"Attempting connect to channel is: {bootNodeIP} and my ip is {localIP}");
            var channel = GrpcChannel.ForAddress(bootNodeIP);
            var client = new NodeToNodeConnection.NodeToNodeConnectionClient(channel);
            var reply = client.Disconnect(new DisconnectRequest() { ClientAddr = $"http://{localIP}:{options.RPCPort}" });
            logger.LogInformation("BootNode says: " + reply.Message);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogInformation($"ERROR: {ex.Message}");
            if (ex.InnerException != null)
                logger.LogInformation($"INNER EXCEPTION: {ex.InnerException}");
            else
                logger.LogInformation("no more details");

            return false;
        }

    }
}
