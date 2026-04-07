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

app.MapMcp();

app.Run();
