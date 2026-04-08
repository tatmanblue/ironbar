# Task: MCP Consumer Authentication

## Problem Statement

Validate that consumers (AI agents, Claude, etc.) of the MCP server are properly authenticated
before they can invoke tools that write to or read sensitive data from the node.

---

## Current State

There are two auth boundaries in the system:

| Boundary | Current state |
|---|---|
| MCP server → boot node | Solved. The MCP server holds `IRONBAR_MCP_*` API keys and passes them on every gRPC call. The node validates them per-tier (read / read-details / write / admin). |
| AI agent → MCP server | Open. Any agent that can reach port 5100 can invoke any tool. There is no per-agent identity check. |

This task is exclusively about the second boundary — agent → MCP server.

---

## Constraints

- No 3rd party authentication dependencies (no OAuth libraries, identity providers, JWT packages, etc.)
- Solution must be implementable within the MCP server itself
- Must not require exposing node API keys to agents (the MCP server exists precisely to hold those keys on behalf of agents)

---

## What the MCP Protocol Provides

Before choosing a solution, it is important to understand what MCP offers for passing credentials:

- MCP tool calls carry only the arguments defined in the tool's input schema — there is no built-in
  auth header or credential slot in the protocol itself.
- Credentials can be passed in one of two ways:
  1. **As a tool argument** — the agent includes a token field in each tool call. This requires
     every tool schema to declare the token parameter, which leaks the auth model into the tool
     surface and is awkward for agents.
  2. **As an HTTP header on the MCP request** — the agent (or MCP host) includes a custom header
     (e.g. `Authorization` or `X-IronBar-Token`) on the HTTP POST to the MCP endpoint. The MCP
     server inspects the header before dispatching the tool. This is invisible to the tool schemas
     and cleaner for agents.

Option 2 (HTTP header) is the preferred approach.

---

## Proposed Solution: Pre-Shared Token via HTTP Header

The MCP server validates a pre-shared token on every inbound tool call request before forwarding
to the boot node. Agents that do not supply a valid token receive an error response; the gRPC call
is never made.

### How it works

1. The MCP server is configured with one or more allowed tokens via an environment variable
   (e.g. `IRONBAR_MCP_ACCESS_TOKENS`, comma-separated to support multiple agents).
2. On each HTTP POST to the MCP endpoint, the server checks for an `Authorization: Bearer <token>`
   header (or a custom header — TBD during implementation).
3. If the token is absent or invalid, the MCP server returns an error without invoking the tool.
4. If valid, the request proceeds normally; the node never sees the agent token.

### What this does not solve

- Per-agent identity or audit logs (all valid tokens are treated equally).
- Token rotation (tokens are static env-var values).

These are acceptable limitations for the initial implementation. Per-agent identity (e.g.
DID-based or signed tokens) is noted in the MCP Server Proposal as future work.

---

## Exemptions

**Decided:** `ironbar_list_blocks` is exempt from the agent token check. It is public at both
the gRPC layer (no API key required) and the MCP layer. All other tools require a valid token.

---

## Work Items

1. Confirm that the MCP framework (`ModelContextProtocol.AspNetCore`) exposes inbound HTTP headers
   to middleware or request context, and identify the correct hook point for the token check.
2. Add `IRONBAR_MCP_ACCESS_TOKENS` to `McpConfiguration.cs` and `src/mcp/.env`.
3. Implement token validation middleware in `Program.cs` — reject requests with missing or invalid
   tokens before they reach the tool handlers. Exempt `ironbar_list_blocks` from the check.
4. Update `MCP_SERVER_PROPOSAL.md` security section to reflect the implemented auth model.
5. Test with a valid token, an invalid token, and no token.