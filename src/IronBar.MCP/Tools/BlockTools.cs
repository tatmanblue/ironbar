using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Node;

namespace IronBar.MCP.Tools;

[McpServerToolType]
public sealed class BlockTools(BlockHandlingClient client)
{
    [McpServerTool(Name = "ironbar_list_blocks")]
    [Description(
        "Lists all blocks in the Iron Bar ledger. Returns the block ID, hash, and status " +
        "for each block. No API key is required — this tool is publicly accessible.")]
    public async Task<string> ListBlocks()
    {
        ListBlocksReply reply = await client.ListBlocksAsync();
        var blocks = reply.Blocks.Select(b => new
        {
            blockId = b.BlockId,
            blockHash = b.BlockHash,
            status = b.Status
        });
        return JsonSerializer.Serialize(blocks);
    }

    [McpServerTool(Name = "ironbar_get_block")]
    [Description(
        "Retrieves a specific block from the Iron Bar ledger, including its full transaction data. " +
        "Requires the MCP server to be configured with a read-details API key " +
        "(IRONBAR_MCP_READ_DETAILS_API_KEY).")]
    public async Task<string> GetBlock(
        [Description("The numeric ID of the block to retrieve.")] int blockId)
    {
        ReadBlockReply reply = await client.GetBlockAsync(blockId);
        var block = new
        {
            blockId = reply.BlockId,
            blockHash = reply.BlockHash,
            status = reply.Status,
            transactionData = reply.BlockData,
            nonce = reply.Nonce,
            createdOn = reply.CreatedOn,
            parentHash = reply.ParentHash,
            parentId = reply.ParentId,
            referenceId = reply.ReferenceId
        };
        return JsonSerializer.Serialize(block);
    }

    [McpServerTool(Name = "ironbar_create_block")]
    [Description(
        "Submits raw string data to create a new block in the Iron Bar ledger. The boot node " +
        "assigns validation. Agents should serialize any structured data to JSON before calling " +
        "this tool. Requires the MCP server to be configured with a write API key " +
        "(IRONBAR_MCP_WRITE_API_KEY).")]
    public async Task<string> CreateBlock(
        [Description("The raw string data to store in the new block.")] string data)
    {
        CreateBlockReply reply = await client.CreateBlockAsync(data);
        var result = new
        {
            blockId = reply.BlockId,
            blockHash = reply.BlockHash,
            status = reply.Status
        };
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool(Name = "ironbar_list_nodes")]
    [Description(
        "Lists all child nodes currently connected to the Iron Bar boot node, including their " +
        "name, version, and connection state. Requires the MCP server to be configured with " +
        "an admin API key (IRONBAR_MCP_ADMIN_API_KEY).")]
    public async Task<string> ListNodes()
    {
        ListNodesReply reply = await client.ListNodesAsync();
        var nodes = reply.Nodes.Select(n => new
        {
            name = n.Name,
            version = n.Version,
            state = n.State
        });
        return JsonSerializer.Serialize(nodes);
    }
}
