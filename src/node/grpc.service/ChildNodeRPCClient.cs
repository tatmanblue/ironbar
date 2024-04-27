using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using core.Utility;
using Grpc.Core;
using Grpc.Net.Client;
using IConfiguration = core.IConfiguration;

namespace Node.grpc.service; 

/// <summary>
/// ChildNodeRPCClient is used by child nodes to communicate with a bootnode
/// through the NodeToNodeConnection.proto definition  
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
            string localIP = $"{IpAddressUtility.LocalIP()}";
            string bootNodeIP = $"{options.BootAddress}";
            

            logger.LogInformation($"Attempting connect to channel is: {bootNodeIP} and my ip is {localIP}");

            var channel = GrpcChannel.ForAddress(bootNodeIP);
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            
            var channelOptions = new GrpcChannelOptions();
            // channelOptions.Credentials = ChannelCredentials.Insecure;
            /*
            channelOptions.HttpClient = new HttpClient()
            {
                BaseAddress = new Uri(bootNodeIP),
                DefaultRequestVersion = new Version(1, 1),
                
            };
            */
            /*
            channelOptions.HttpHandler = new HttpClientHandler()
            {
                SslProtocols = SslProtocols.Tls12
            };
            */
            /*
            logger.LogDebug($"Channel options are {channelOptions.ToString()}");
            var channel = GrpcChannel.ForAddress(bootNodeIP, channelOptions);
            */
            var client = new NodeToNodeConnection.NodeToNodeConnectionClient(channel);
            var reply = client.Connect(new ConnectRequest()
            {
                ClientAddr = CreateClientAddress(),
                FriendlyName = options.FriendlyName,
                NodeVersion = options.Version
            });
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
            var reply = client.Disconnect(new DisconnectRequest()
            {
                ClientAddr = CreateClientAddress(),
                FriendlyName = options.FriendlyName
            });
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

    private string CreateClientAddress()
    {
        string localIP = $"{IpAddressUtility.LocalIP()}";
        return $"http://{localIP}:{options.RPCPort}";
    }
}
