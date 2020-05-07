using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using node.General;

namespace node
{
    /// <summary>
    /// this is currently used to use GRPC generated code for a child node to talk
    /// to the bootnode.  It is used by ClientNodeService
    /// TODO: not sure but maybe this should be consolidated into ClientNodeService
    /// </summary>
    public class NodeRPCClient
    {
        private readonly IOptions _options;
        public NodeRPCClient(IOptions options)
        {
            _options = options;
        }

        public async Task<bool> ConnectToBootNode(int delay = 2000)
        {
            {
                try
                {
                    Task.Delay(delay).Wait();
                    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                    Console.WriteLine($"Attempting connect to channel is: http://localhost:{_options.ServerRPCPort}");
                    var channel = GrpcChannel.ForAddress($"http://localhost:{_options.ServerRPCPort}");
                    var client = new BootNode.BootNodeClient(channel);
                    var reply = client.AddLink(new LinkRequest { ClientAddr = $"http://localhost:{_options.RPCPort}" });
                    Console.WriteLine("BootNode says: " + reply.Message);

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    if (ex.InnerException != null)
                        Console.WriteLine($"INNER EXCEPTION: {ex.InnerException}");
                    else
                        Console.WriteLine("no more details");

                    return false;
                }
            }
        }
    }
}
