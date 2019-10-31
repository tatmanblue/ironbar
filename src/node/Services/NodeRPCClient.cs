using System;
using System.Threading.Tasks;
using Grpc.Net.Client;

namespace node
{
    public class NodeRPCClient
    {
        private readonly Options _options;
        public NodeRPCClient(Options options)
        {
            _options = options;
        }

        public async Task<bool> ConnectToBootNode()
        {
            {
                try
                {
                    Task.Delay(2000).Wait();
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
