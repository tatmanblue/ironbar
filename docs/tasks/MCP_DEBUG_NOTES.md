# MCP Server Debugging Notes

**Date:** 2026-04-07  
**Branch:** `feature/mcp-server`  
**Status:** In progress — `ironbar_create_block` unresolved

---

## What Works

| Tool | Status | Notes |
|---|---|---|
| `ironbar_list_blocks` | Working | Returns genesis block metadata |
| `ironbar_get_block` | Working | Returns full block data including transactionData |
| `ironbar_create_block` | Failing | See below |
| `ironbar_list_nodes` | Untested | |

---

## MCP Server Configuration

- Runs on port **5100** (set via `IRONBAR_MCP_PORT` in `src/mcp/.env`)
- Boot node at `http://localhost:50051`
- Uses Streamable HTTP transport (not SSE) — `ModelContextProtocol.AspNetCore` 1.2.0
- MCP endpoint: `POST http://localhost:5100/` with `Mcp-Session-Id` header after initialize

---

## Fixes Made During This Session

### 1. ApiKeyManager — MCP key support (`node/General/ApiKeyManager.cs`)

The node's `ApiKeyManager` now loads and checks MCP-specific API keys alongside the standard keys. Each method accepts either the standard key or its MCP counterpart:

| Method | Accepts |
|---|---|
| `IsReadAllowed` | `IRONBAR_READ_API_KEY`, `IRONBAR_READ_DETAILS_API_KEY`, `IRONBAR_MCP_READ_API_KEY`, `IRONBAR_MCP_READ_DETAILS_API_KEY` |
| `IsReadDetailsAllowed` | `IRONBAR_READ_DETAILS_API_KEY`, `IRONBAR_MCP_READ_DETAILS_API_KEY` |
| `IsWriteAllowed` | `IRONBAR_WRITE_API_KEY`, `IRONBAR_MCP_WRITE_API_KEY` |
| `IsAdmin` | `IRONBAR_ADMIN_API_KEY`, `IRONBAR_MCP_ADMIN_API_KEY` |

**Required configuration:** The boot node's `.env` must include the `IRONBAR_MCP_*` keys with the values that the MCP server will send. These should match what is set in `src/mcp/.env`.

### 2. Aspire workload deprecation (`AspireHost/AspireHost.csproj`)

Added `<Sdk Name="Aspire.AppHost.Sdk" Version="9.5.2" />` to satisfy the .NET 10 SDK deprecation check (`NETSDK1228`). Updated `Aspire.Hosting.AppHost` to `9.5.2` and all ServiceDefaults packages to their matching versions.

---

## `ironbar_create_block` Failure

### Symptom

The MCP tool returns `"An error occurred invoking 'ironbar_create_block'"`.

### Root Cause (identified)

The block IS created successfully — the node logs show the block string being assembled. The failure occurs **after** creation during the BFT validation step:

```
Unhandled exception. System.NullReferenceException: Object reference not set to an instance of an object.
   at Node.Ledger.BootNodePhysicalBlockValidator.Validate(ILedgerIndexManager indexes, ILedgerPhysicalBlock block)
      in node/Ledger/BootNodePhysicalBlockValidator.cs:line 23
   at Node.Ledger.Ledger.ValidateBlock(ILedgerPhysicalBlock block)
      in node/Ledger/Ledger.cs:line 253
   at Node.Ledger.Ledger.AdvanceBlock(ILedgerPhysicalBlock pb, BlockStatus status)
      in node/Ledger/Ledger.cs:line 211
   at Node.Ledger.LedgerManager.AdvanceBlock(ILedgerPhysicalBlock pb, BlockStatus status)
      in node/Ledger/LedgerManager.cs:line 118
   at Node.grpc.service.BootNodeRPCClient.OnBlockCreated(ILedgerPhysicalBlock pb)
      in node/grpc.service/BootNodeRPCClient.cs:line 99
   at Node.Ledger.BootNodeServicesEvents.FireBlockCreated(...)
      in node/Ledger/BootNodeServicesEvents.cs:line 20
```

The block data reaching the node was confirmed in the debug log:
```
'1:2:5196b8...:Unconfirmed:First block created via Iron Bar MCP Server - 2026-04-07:c1688ae...'
```

### Files to Investigate

- `node/Ledger/BootNodePhysicalBlockValidator.cs` — line 23, `NullReferenceException`
- `node/Ledger/Ledger.cs` — lines 211, 253 (`AdvanceBlock`, `ValidateBlock`)
- `node/Ledger/LedgerManager.cs` — line 118 (`AdvanceBlock`)
- `node/grpc.service/BootNodeRPCClient.cs` — line 99 (`OnBlockCreated`)

### Suspected Cause

`BootNodePhysicalBlockValidator.Validate` receives a null reference — likely `indexes` or a property on `block`. This may be related to:
- Running in single-node mode (no child nodes) where the validator may not be fully initialized
- Possible connection between Azure Blob storage config and the validator's index manager

### Context

- The node was initially configured with Azure Blob storage. Switching to filesystem storage was in progress at end of session.
- This exception is pre-existing in the node codebase and is **not introduced by the MCP server**.

---

## Next Steps

1. Read `BootNodePhysicalBlockValidator.cs` to identify what is null at line 23
2. Determine if the null is `indexes` (the `ILedgerIndexManager`) or something on `block`
3. Check `BootNodeRPCClient.cs:99` — `OnBlockCreated` — for how it calls `AdvanceBlock` and whether `indexes` is passed correctly in single-node mode
4. Confirm whether the issue reproduces with filesystem storage after the config switch
5. Once `create_block` is fixed, test `ironbar_list_nodes`
6. Commit all changes on `feature/mcp-server` branch
