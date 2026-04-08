using DotNetEnv;
using IronBar.MCP;
using IronBar.MCP.Tools;

Env.Load();

McpConfiguration config = McpConfiguration.FromEnvironment();

var builder = WebApplication.CreateBuilder(args);

// When IRONBAR_MCP_PORT is set (standalone deployment), bind to that port explicitly.
// When running under Aspire, omit this so Aspire controls the port via ASPNETCORE_URLS.
string? mcpPort = Environment.GetEnvironmentVariable("IRONBAR_MCP_PORT");
if (!string.IsNullOrEmpty(mcpPort) && int.TryParse(mcpPort, out int port))
{
    builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(port));
}

builder.Services.AddSingleton(config);
builder.Services.AddSingleton<BlockHandlingClient>();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<BlockTools>();

var app = builder.Build();

// Agent authentication middleware.
// If IRONBAR_MCP_ACCESS_TOKENS is configured, every tool call except ironbar_list_blocks
// must supply a valid token in the Authorization: Bearer header.
// An empty token set disables enforcement (development / local mode).
if (config.AccessTokens.Count > 0)
{
    app.Use(async (context, next) =>
    {
        // Enable buffering so the MCP framework can still read the body after we peek at it.
        context.Request.EnableBuffering();
        using StreamReader reader = new(context.Request.Body, leaveOpen: true);
        string body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        // Only enforce auth on tools/call requests — protocol messages (initialize,
        // tools/list, etc.) pass through freely.
        if (!body.Contains("\"tools/call\""))
        {
            await next(context);
            return;
        }

        // ironbar_list_blocks is publicly accessible — no token required.
        if (body.Contains("\"ironbar_list_blocks\""))
        {
            await next(context);
            return;
        }

        // All other tool calls require a valid Bearer token.
        string? authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        string? token = authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
            ? authHeader["Bearer ".Length..].Trim()
            : null;

        if (token is null || !config.AccessTokens.Contains(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        await next(context);
    });
}

app.MapMcp();

app.Run();
