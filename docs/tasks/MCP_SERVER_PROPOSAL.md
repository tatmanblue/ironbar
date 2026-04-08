# Proposal: MCP Server for Iron Bar

**Date:** 2026-04-07  
**Status:** Draft / Theoretical  
**Author:** Proposal generated for review

---

## Overview

This document proposes adding a **Model Context Protocol (MCP) server** to Iron Bar that would allow AI agents — such as Claude, Cursor, or any MCP-compatible client — to query the ledger and submit data to create new blocks. The MCP server would act as a translation layer between the AI agent world and the Iron Bar boot node, communicating downstream via the existing gRPC `BlockHandlingApi`.

---

## What Is MCP?

The [Model Context Protocol](https://modelcontextprotocol.io) is an open standard created by Anthropic that defines how AI agents connect to external tools and data sources. An MCP server exposes **tools** (callable functions) and/or **resources** (queryable data) to any MCP-compatible AI client. The protocol transport is typically:

- **stdio** — subprocess model, client spawns the server
- **HTTP/SSE** — network model, server runs as a standalone service

For Iron Bar, the HTTP/SSE transport is the natural fit since the boot node is already a long-running network service.

---

## Feasibility Assessment

**Yes, this is entirely feasible.** Iron Bar already has all the necessary building blocks:

| Requirement | Iron Bar Current State |
|---|---|
| Public API for block creation | `BlockHandlingApi.Create()` gRPC method |
| Public API for block queries | `BlockHandlingApi.Read()`, `List()` gRPC methods |
| API key access control | `ApiKeyManager` — read/write/admin tiers |
| Node status queries | `BlockHandlingApi.ListNodes()` gRPC method |
| Plugin extensibility | `IApiPlugin` / `IGrpcService` interfaces |
| .NET 10.0 runtime | MCP .NET SDK (`ModelContextProtocol` NuGet) available |

The primary work is writing a new project that acts as the MCP server. No changes to the core node are required, though a plugin-based integration is also an option (discussed below).

---

## Proposed Architecture

```
┌─────────────────────────────────────────┐
│           AI Agent / Client             │
│  (Claude, Cursor, custom agent, etc.)   │
└───────────────┬─────────────────────────┘
                │  MCP Protocol (HTTP/SSE or stdio)
                │  JSON-RPC 2.0
                ▼
┌─────────────────────────────────────────┐
│         Iron Bar MCP Server             │
│    (new: src/mcp/ project)              │
│                                         │
│  Tools:                                 │
│    ironbar_list_blocks                  │
│    ironbar_get_block                    │
│    ironbar_create_block                 │
│    ironbar_list_nodes                   │
│                                         │
│  Resources:                             │
│    ironbar://blocks/{id}                │
│    ironbar://blocks/index               │
└───────────────┬─────────────────────────┘
                │  gRPC (HTTP/2)
                │  BlockHandlingApi proto
                ▼
┌─────────────────────────────────────────┐
│         Iron Bar Boot Node              │
│    (existing: src/node/)                │
│                                         │
│  BootNodeBlockApiService                │
│    Create / Read / List / ListNodes     │
└─────────────────────────────────────────┘
```

The MCP server is a **separate, standalone process** — not embedded in the boot node. This keeps concerns cleanly separated and allows the MCP server to be deployed independently (e.g., only expose MCP in environments where AI agent access is desired).

---

## MCP Tools Specification

### `ironbar_list_blocks`

Lists all blocks in the ledger (metadata only — no block data).

**Input:** none  
**Output:** Array of `{ blockId, hash, status, created }`  
**Required API key tier:** Read  
**Maps to:** `BlockHandlingApi.List()`

---

### `ironbar_get_block`

Retrieves a specific block including its transaction data.

**Input:** `{ blockId: number }`  
**Output:** `{ blockId, parentId, referenceId, hash, parentHash, status, timestamp, nonce, transactionData }`  
**Required API key tier:** Read-Details  
**Maps to:** `BlockHandlingApi.Read()`

---

### `ironbar_create_block`

Submits data to create a new block. The boot node assigns validation.

**Input:** `{ data: string }`  
**Output:** `{ blockId, hash, status }`  
**Required API key tier:** Write  
**Maps to:** `BlockHandlingApi.Create()`

---

### `ironbar_list_nodes`

Returns the connected child nodes (useful for agents monitoring cluster health).

**Input:** none  
**Output:** Array of `{ nodeId, address, status }`  
**Required API key tier:** Admin  
**Maps to:** `BlockHandlingApi.ListNodes()`

---

## MCP Resources Specification

Resources allow AI clients to subscribe to or directly address Iron Bar data as URIs.

| Resource URI | Description | API key tier |
|---|---|---|
| `ironbar://blocks/index` | Full block index (same as `List`) | Read |
| `ironbar://blocks/{id}` | Single block with data | Read-Details |

Resources are optional for an initial implementation — tools alone are sufficient.

---

## Implementation Plan

### New Project: `src/IronBar.MCP`

A new .NET 10.0 console application (or minimal API web app) using the [ModelContextProtocol NuGet package](https://www.nuget.org/packages/ModelContextProtocol). The project lives at `src/IronBar.MCP/IronBar.MCP.csproj` alongside the other top-level projects in the solution.

**Dependencies:**
- `ModelContextProtocol` — MCP server SDK for .NET
- `Grpc.Net.Client` — already used by child nodes, patterns exist
- `Google.Protobuf` + `IronBar.Node.Grpc` — generated client stubs (reference `node` project or extract proto files)

**Configuration (environment variables):**

| Variable | Purpose |
|---|---|
| `IRONBAR_BOOTNODE_ADDRESS` | gRPC address of the boot node (e.g., `http://localhost:50051`) |
| `IRONBAR_MCP_READ_API_KEY` | API key for read operations |
| `IRONBAR_MCP_READ_DETAILS_API_KEY` | API key for detailed reads |
| `IRONBAR_MCP_WRITE_API_KEY` | API key for block creation |
| `IRONBAR_MCP_ADMIN_API_KEY` | API key for node listing (optional) |
| `IRONBAR_MCP_PORT` | Port for HTTP/SSE transport (default: `5100`) |

---

### Phase 1 — Core MCP Server (Estimated scope: medium)

1. **Create project** `src/IronBar.MCP/IronBar.MCP.csproj` and add it to `src/ironbar.sln`
   - Target `net10.0`
   - Add `ModelContextProtocol`, `Grpc.Net.Client`, proto references

2. **gRPC client wrapper** (`BlockHandlingClient.cs`)
   - Thin wrapper around the auto-generated `BlockHandlingApi.BlockHandlingApiClient`
   - Reads configuration for boot node address and API keys
   - Handles channel lifecycle

3. **MCP tool handlers**
   - `ListBlocksTool` — calls `List()`, returns JSON array
   - `GetBlockTool` — calls `Read()`, returns JSON object
   - `CreateBlockTool` — calls `Create()`, returns new block summary
   - `ListNodesTool` — calls `ListNodes()`, returns JSON array

4. **Tiered auth middleware**
   - `ironbar_list_blocks` — no API key required (public)
   - All other tools — validate API key from MCP request context before forwarding to gRPC

5. **Server startup** (`Program.cs`)
   - Register MCP server with HTTP/SSE transport
   - Wire tools to DI container
   - Configure Kestrel for the MCP port

---

### Phase 2 — Aspire Integration (Estimated scope: small)

Add the MCP server as a resource in `src/AspireHost/` so it starts alongside the boot node and child nodes in local development.

- Register `IronBar.MCP` project resource
- Wire `IRONBAR_BOOTNODE_ADDRESS` to reference the boot node's endpoint
- Expose the MCP port in the Aspire dashboard

---

### Phase 3 — Plugin Alternative (Optional / Lower priority)

As an alternative to a standalone process, the MCP server could be packaged as an `IApiPlugin` that loads into the boot node itself. This would:

- Eliminate the separate process
- Allow the MCP server to call `ILedgerManager` directly (no gRPC hop)
- Require extending `IGrpcService` or adding HTTP/SSE listener registration to the plugin host

This approach is more invasive but reduces operational complexity. It is worth considering if the standalone deployment model proves cumbersome.

---

## Security Considerations

1. **API key exposure** — The MCP server holds API keys to talk to the boot node. These must be kept out of agent-visible context (not returned in tool descriptions or error messages). New API keys dedicated to the MCP server should be provisioned — separate from any keys used by other clients.

2. **Scoping by intent** — The MCP server should be configured with only the minimum key tier needed for its intended use. An MCP server intended for read-only agents should not hold the Write key. Operators can deploy multiple MCP server instances with different key scopes if mixed access is needed.

3. **Agent authentication** — The MCP server enforces a pre-shared bearer token check on all
   inbound tool calls except `ironbar_list_blocks`. Agents must supply a valid token in the
   `Authorization: Bearer <token>` HTTP header; requests without a valid token receive `401
   Unauthorized` and the gRPC call is never made. Valid tokens are configured via
   `IRONBAR_MCP_ACCESS_TOKENS` (comma-separated). An empty value disables enforcement for local
   development. Agent tokens are entirely separate from the node API keys — agents never see the
   keys the MCP server uses to talk to the boot node.

4. **Anonymous / unauthenticated agents** — `ironbar_list_blocks` is publicly accessible with no
   bearer token required. It exposes only block metadata (ID, hash, status) and no transaction
   data, mirroring the public ledger transparency common in blockchain systems. All other tools
   require a valid bearer token.

5. **Agent identity (future work)** — Pre-shared bearer tokens are sufficient for the initial
   implementation. A more expressive "agent identity" model (e.g., signed JWTs, DID-based
   identity) may be added in a future iteration to support auditable per-agent access logs.

6. **Network boundary** — The MCP server should only be accessible from trusted hosts. In production, it should not be publicly exposed without careful consideration of what the anonymous read tier reveals.

7. **Block data size** — Iron Bar currently imposes no size limit on block data. The MCP server will follow the same convention and not add its own limit, keeping behavior consistent across all clients.

8. **No direct node-to-node access** — The MCP server communicates only with the boot node via `BlockHandlingApi`. It never touches `NodeToNodeConnection` — that protocol remains internal to the cluster.

---

## Decisions

The following design decisions have been made for the initial implementation:

1. **Streaming** — Pull-based querying via `ironbar_list_blocks` and `ironbar_get_block` is sufficient. Streaming / push-based block subscription can be added in a future iteration if there is demand.

2. **Block data format** — `ironbar_create_block` accepts raw strings. This matches the current `blockData` field on the gRPC API and keeps the tool simple. Agents that want to store structured data should serialize it to JSON themselves before calling the tool.

3. **Deployment model** — The MCP server is a **standalone process**, not a plugin. This keeps concerns separated and allows independent deployment. A plugin-based integration can be explored later if the standalone model proves cumbersome.

4. **Docker image** — The MCP server ships as a **separate Docker image**. Not all Iron Bar deployments will require MCP functionality; keeping it separate allows users to opt in without adding weight to the core node image.

---

## What This Enables

Once implemented, any MCP-compatible AI agent could:

- **Audit the ledger** — "Show me all blocks created in the last hour" (using `list_blocks` + filtering)
- **Verify a specific record** — "What is the transaction data in block 42?" (using `get_block`)
- **Write new records** — "Record that user X completed action Y" (using `create_block`)
- **Monitor cluster health** — "How many nodes are currently connected?" (using `list_nodes`)
- **Browse without credentials** — An unknown agent can call `ironbar_list_blocks` to discover what is on the ledger (block IDs, hashes, statuses) without holding an API key and without exposing any transaction data.

This makes Iron Bar directly consumable as a persistence backend by AI agents without any custom integration code on the agent side — only an MCP configuration entry pointing at the server.

---

## Summary

| Item | Assessment |
|---|---|
| Feasibility | High — existing gRPC API covers all required operations |
| Risk | Low — additive change, no core node modifications required |
| Scope | New project (~300-500 lines), Aspire integration (~50 lines) |
| Dependencies | `ModelContextProtocol` NuGet package (Anthropic-maintained) |
| Breaking changes | None |
