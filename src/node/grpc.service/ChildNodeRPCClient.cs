using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using core.Ledger;
using core.Utility;
using Grpc.Core;
using Grpc.Net.Client;
using Node.General;
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
    private readonly ILedgerManager ledgerManager;
    
    public ChildNodeRPCClient(IServiceProvider serviceProvider)
    {
        this.options = serviceProvider.GetRequiredService<IConfiguration>();
        this.logger = serviceProvider.GetRequiredService<ILogger<ChildNodeRPCClient>>();
        this.ledgerManager = serviceProvider.GetRequiredService<ILedgerManager>();
    }

    /// <summary>
    /// Connects to the boot node.  There are a couple of steps.
    ///
    /// 1 - simply say hi to the boot node
    /// 2 - request the index and validate it
    /// 3 - send the index validation to the server for finally verification
    ///
    /// Passing these steps means the client node is now approved for participating
    /// in all ledger transactions 
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    public async Task<bool> ConnectToBootNode(int delay = 2000)
    {
        try
        {
            Task.Delay(delay).Wait();
            string childSvcAddress = CreateChildSvcAddress();
            string bootNodeIP = $"{options.BootAddress}";
            

            logger.LogInformation($"Attempting connect to channel is: {bootNodeIP} and child service address is '{childSvcAddress}' and node is named '{options.FriendlyName}'");
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            
            var channelOptions = new GrpcChannelOptions();
            // channelOptions.Credentials = ChannelCredentials.Insecure;
            var channel = GrpcChannel.ForAddress(bootNodeIP, channelOptions);
            
            
            // Step 1 -- just "say hi" to boot node
            var client = new NodeToNodeConnection.NodeToNodeConnectionClient(channel);
            ConnectRequest request = new ConnectRequest()
            {
                ClientAddr = childSvcAddress,
                FriendlyName = options.FriendlyName,
                NodeVersion = options.Version
            };
            var reply = client.Connect(request);
            logger.LogInformation("BootNode says: " + reply.Message);

            
            // Step 2 -- get the indexes from the boot node
            var indexes = client.ClientRequestIndex(request);
            string verification = ledgerManager.CreateIndexesVerification(indexes.Indexes);
            logger.LogInformation($"Received index from boot node.  Verification hash: {verification}");

            
            // Step 3 - send verification to bootnode and process bootnode response
            ValidateIndexRequest validateIndexRequest = new()
            {
                FriendlyName = options.FriendlyName,
                ClientAddr = request.ClientAddr,
                Nonce = indexes.Nonce,
                Evaluated = indexes.Indexes.Count.ToString(),
                Validation = verification
            };
            var validateResponse = client.ClientValidatesIndex(validateIndexRequest);
            if (Globals.APPROVED != validateResponse.Approved)
            {
                logger.LogCritical("Failed to connect and sync with boot node");
                return false;
            }
            
            ledgerManager.SyncIndex(indexes.Indexes, verification);
            logger.LogInformation("Indexes sync with boot node");
            
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
            string childSvcAddress = CreateChildSvcAddress();
            string bootNodeIP = $"{options.BootAddress}";
            logger.LogInformation($"Attempting connect to channel is: {bootNodeIP} and child service address is: '{childSvcAddress}'");
            var channel = GrpcChannel.ForAddress(bootNodeIP);
            var client = new NodeToNodeConnection.NodeToNodeConnectionClient(channel);
            var reply = client.Disconnect(new DisconnectRequest()
            {
                ClientAddr = childSvcAddress,
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

    private string CreateChildSvcAddress()
    {
        string hostAddress =  $"{IpAddressUtility.LocalIP()}";
        if (false == string.IsNullOrEmpty(options.ServiceAddress))
        {
            hostAddress = options.ServiceAddress;
        }
        
        return $"http://{hostAddress}:{options.RPCPort}";
    }
}
