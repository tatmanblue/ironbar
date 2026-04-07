using Grpc.Net.Client;
using Node;

namespace IronBar.MCP;

public sealed class BlockHandlingClient : IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly BlockHandlingApi.BlockHandlingApiClient _client;
    private readonly McpConfiguration _config;

    public BlockHandlingClient(McpConfiguration config)
    {
        _config = config;
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        _channel = GrpcChannel.ForAddress(config.BootNodeAddress);
        _client = new BlockHandlingApi.BlockHandlingApiClient(_channel);
    }

    public async Task<ListBlocksReply> ListBlocksAsync()
    {
        return await _client.ListAsync(new ListBlocksRequest
        {
            ApiKey = _config.ReadApiKey,
            IncludeBody = false
        });
    }

    public async Task<ReadBlockReply> GetBlockAsync(int blockId)
    {
        if (string.IsNullOrEmpty(_config.ReadDetailsApiKey))
            throw new InvalidOperationException(
                "IRONBAR_MCP_READ_DETAILS_API_KEY is not configured on this MCP server.");

        return await _client.ReadAsync(new ReadBlockRequest
        {
            ApiKey = _config.ReadDetailsApiKey,
            BlockId = blockId.ToString()
        });
    }

    public async Task<CreateBlockReply> CreateBlockAsync(string data)
    {
        if (string.IsNullOrEmpty(_config.WriteApiKey))
            throw new InvalidOperationException(
                "IRONBAR_MCP_WRITE_API_KEY is not configured on this MCP server.");

        return await _client.CreateAsync(new CreateBlockRequest
        {
            ApiKey = _config.WriteApiKey,
            BlockData = data
        });
    }

    public async Task<ListNodesReply> ListNodesAsync()
    {
        if (string.IsNullOrEmpty(_config.AdminApiKey))
            throw new InvalidOperationException(
                "IRONBAR_MCP_ADMIN_API_KEY is not configured on this MCP server.");

        return await _client.ListNodesAsync(new ListNodesRequest
        {
            ApiKey = _config.AdminApiKey
        });
    }

    public void Dispose() => _channel.Dispose();
}
