# Iron Bar MCP Server

The Iron Bar MCP server exposes the ledger to AI agents via the
[Model Context Protocol](https://modelcontextprotocol.io). Any MCP-compatible AI host — Claude
Desktop, Cursor, VS Code, or a custom integration — can connect to it and use the ledger as a
persistence backend without writing any custom integration code.

---

## How it works

The MCP server is a standalone process that sits between AI agents and the Iron Bar boot node.

```
AI Agent (Claude, Cursor, etc.)
        │  MCP Protocol (HTTP)
        ▼
Iron Bar MCP Server  (port 5100)
        │  gRPC
        ▼
Iron Bar Boot Node   (port 50051)
```

The agent calls tools (`ironbar_list_blocks`, `ironbar_create_block`, etc.). The MCP server
translates those calls into gRPC requests to the boot node and returns the results. The agent
never communicates with the boot node directly and never sees the node's API keys.

---

## Available Tools

| Tool | Description | Auth required |
|---|---|---|
| `ironbar_list_blocks` | Lists all blocks (ID, hash, status). No transaction data. | No |
| `ironbar_get_block` | Returns a specific block including transaction data. | Yes |
| `ironbar_create_block` | Submits string data to create a new block. | Yes |
| `ironbar_list_nodes` | Lists connected child nodes and their state. | Yes |

---

## Running the MCP Server

The MCP server is configured entirely through environment variables. Copy `src/mcp/.env` and
set values appropriate for your environment before starting.

### Environment variables

| Variable | Required | Description |
|---|---|---|
| `IRONBAR_BOOTNODE_ADDRESS` | Yes | gRPC address of the boot node. Default: `http://localhost:50051` |
| `IRONBAR_MCP_PORT` | No | Port the MCP server listens on. Default: `5100`. Omit when running under Aspire. |
| `IRONBAR_MCP_READ_API_KEY` | Yes | Node API key for read operations (`ironbar_list_blocks`). |
| `IRONBAR_MCP_READ_DETAILS_API_KEY` | Yes | Node API key for detailed reads (`ironbar_get_block`). |
| `IRONBAR_MCP_WRITE_API_KEY` | Yes | Node API key for block creation (`ironbar_create_block`). |
| `IRONBAR_MCP_ADMIN_API_KEY` | No | Node API key for node listing (`ironbar_list_nodes`). |
| `IRONBAR_MCP_ACCESS_TOKENS` | No | Comma-separated bearer tokens agents must supply. Empty = no enforcement (dev mode). |

### Starting the server

```bash
cd src/mcp
dotnet run
```

Or with Aspire (starts alongside the boot node and child nodes automatically):

```bash
cd src/AspireHost
dotnet run
```

---

## Configuring an Agent

The bearer token and server URL are configured at the **MCP host** level — in the host's server
configuration file. The agent itself is unaware of the token; the host injects the
`Authorization` header on every HTTP request automatically.

### Claude Desktop

Edit `~/Library/Application Support/Claude/claude_desktop_config.json` (macOS) or
`%APPDATA%\Claude\claude_desktop_config.json` (Windows):

```json
{
  "mcpServers": {
    "ironbar": {
      "type": "http",
      "url": "http://localhost:5100/",
      "headers": {
        "Authorization": "Bearer <your-token>"
      }
    }
  }
}
```

Restart Claude Desktop after saving. The Iron Bar tools will appear in the tool list.

### Cursor

Edit `.cursor/mcp.json` in your project root (or the global Cursor MCP config):

```json
{
  "mcpServers": {
    "ironbar": {
      "type": "http",
      "url": "http://localhost:5100/",
      "headers": {
        "Authorization": "Bearer <your-token>"
      }
    }
  }
}
```

### Claude Code CLI

```bash
claude mcp add --transport http ironbar http://localhost:5100/
```

To include the bearer token, add it to the headers option per the Claude Code MCP documentation.

### Custom / programmatic clients

Set the `Authorization: Bearer <your-token>` header on every HTTP POST to the MCP endpoint.
The `ironbar_list_blocks` tool does not require the header — all other tool calls do.

---

## Authentication

### Agent → MCP server

Access is controlled by bearer tokens configured in `IRONBAR_MCP_ACCESS_TOKENS`.

- Set `IRONBAR_MCP_ACCESS_TOKENS` to a comma-separated list of tokens (e.g. `token-a,token-b`).
- Each MCP host instance is given one token from this list.
- To revoke an agent's access, remove its token from the list and restart the MCP server.
- Leave the variable empty to disable enforcement (useful for local development).

`ironbar_list_blocks` is always exempt — it is publicly accessible with no token required.

### MCP server → boot node

The MCP server authenticates to the boot node using the `IRONBAR_MCP_*` API keys. These are
never exposed to agents. The node's `ApiKeyManager` recognises MCP-specific keys alongside
standard keys, so MCP access can be revoked independently of other clients.

---

## Security notes

Please note that the bearer token authentication implemented by the MCP server is a simple shared secret mechanism. 
It is not as robust as a full OAuth implementation and should be used with caution, especially in production 
environments. Here are some security best practices to consider:

- Provision dedicated API keys for the MCP server — do not reuse keys from other clients.
- Give each MCP host instance its own bearer token so access can be revoked per-agent.
- The MCP server should not be publicly exposed without careful consideration; anyone who can
  reach port 5100 can call `ironbar_list_blocks` without a token.
- Transaction data is only returned by `ironbar_get_block`, which requires a valid token.
