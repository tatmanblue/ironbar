using System.Net;
using System.Net.Sockets;
using Grpc.Net.Client;
using IConfiguration = core.IConfiguration;

namespace Node.grpc.service;

/// <summary>
/// This client handles messages to child nodes from the boot node
/// </summary>
public class BootNodeRPCClient
{
    private readonly ILogger<BootNodeRPCClient> _logger;
    private readonly IConfiguration _options;

    public BootNodeRPCClient(IConfiguration options, ILogger<BootNodeRPCClient> logger)
    {
        _options = options;
        _logger = logger;
    }


    public async Task<bool> SendShuttingDownMessage(string clientNodeAddress)
    {
        try
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            string localIP = $"{LocalIPAddress()}";                
            _logger.LogInformation($"Attempting connect to channel is: {clientNodeAddress} and my ip is {localIP}");
            var channel = GrpcChannel.ForAddress(clientNodeAddress);
            var client = new BootNode.BootNodeClient(channel);
            var reply = client.SendSimpleMessage(new SimpleMessage { ClientAddr = $"http://{localIP}:{_options.RPCPort}" });
            _logger.LogInformation("ChildNode reply: " + reply.Message);

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
