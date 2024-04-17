using System.Net;
using System.Net.Sockets;
using core.Ledger;
using core.Utility;
using Grpc.Net.Client;
using Node.General;
using Node.Interfaces;
using IConfiguration = core.IConfiguration;

namespace Node.grpc.service;

/// <summary>
/// TODO this service is not part of the GRPC implementation but it does consume GRPC implementations
/// 
/// BootNodeRPCClient handles messages to child nodes from the boot node
/// </summary>
public class BootNodeRPCClient
{
    private readonly ILogger<BootNodeRPCClient> _logger;
    private readonly IConfiguration _options;

    public BootNodeRPCClient(IConfiguration options, ILogger<BootNodeRPCClient> logger, IServicesEventSub eventSub)
    {
        _options = options;
        _logger = logger;
        
        eventSub.OnBlockCreated += OnBlockCreated;
        eventSub.OnChildNodeConnected += OnChildNodeCreated;
    }

    private void OnChildNodeCreated(ChildNodeConnection conn)
    {
        _logger.LogInformation("BootNodeRPCClient is handling a new node");
    }

    private void OnBlockCreated(ILedgerPhysicalBlock pb)
    {
        _logger.LogInformation("BootNodeRPCClient got OnBlockCreated()");
    }

    public async Task<bool> SendShuttingDownMessage(string clientNodeAddress)
    {
        try
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            string localIP = $"{IpAddressUtility.LocalIP()}";                
            _logger.LogInformation($"Attempting connect to channel is: {clientNodeAddress} and my ip is {localIP}");
            var channel = GrpcChannel.ForAddress(clientNodeAddress);
            var client = new NodeToNodeConnection.NodeToNodeConnectionClient(channel);
            var reply = client.Disconnect(new DisconnectRequest() { ClientAddr = $"http://{localIP}:{_options.RPCPort}" });
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


}
