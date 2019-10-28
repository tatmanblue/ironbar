using System;
using System.Threading.Tasks;
using Grpc.Net.Client;

namespace noderpc
{
    public class TestClient
    {
        private readonly Options _options;
        public TestClient(Options options)
        {
            _options = options;
        }

        public async Task ConnectToBootNode()
        {
            var channel = GrpcChannel.ForAddress($"https://localhost:{_options.ServerRPCPort}");
            var client = new boot_node.BootNodeClient(channel);
            var reply = await client.AddLink(
                              new LinkRequest { ClientPort = "1" });
            Console.WriteLine("BootNode says: " + reply.Message);
        }
    }
}
