﻿using System.Net;
using System.Net.Sockets;
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
            // string localIP = $"{LocalIPAddress()}";
            string localIP = "localhost";
            // TODO: bootNode may not be on the same host, and may be using https
            string bootNodeIP = $"http://localhost:{options.ServerRPCPort}";
            logger.LogInformation($"Attempting connect to channel is: {bootNodeIP} and my ip is {localIP}");
            var channel = GrpcChannel.ForAddress(bootNodeIP);
            var client = new BootNode.BootNodeClient(channel);
            var reply = client.AddLink(new LinkRequest { ClientAddr = $"http://{localIP}:{options.RPCPort}" });
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
            // string localIP = $"{LocalIPAddress()}";
            string localIP = "localhost";
            // TODO: bootNode may not be on the same host, and may be using https
            string bootNodeIP = $"http://localhost:{options.ServerRPCPort}";
            logger.LogInformation($"Attempting connect to channel is: {bootNodeIP} and my ip is {localIP}");
            var channel = GrpcChannel.ForAddress(bootNodeIP);
            var client = new BootNode.BootNodeClient(channel);
            var reply = client.SendSimpleMessage(new SimpleMessage { ClientAddr = $"http://{localIP}:{options.RPCPort}" });
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