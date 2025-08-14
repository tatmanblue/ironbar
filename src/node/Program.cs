using System.Net;
using System.Security.Authentication;
using core.Ledger;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Node.General;
using Node.Ledger;
using Node.grpc.service;


ConfigurationOptions configurationOptions = ConfigurationOptions.FromEnvironment();

var builder = WebApplication.CreateBuilder(args);

// adds the dotnet Aspire service defaults
builder.AddServiceDefaults();
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.Format = Microsoft.Extensions.Logging.Console.ConsoleLoggerFormat.Default;
});

builder.Services.AddHealthChecks();
builder.Services.AddGrpc().AddServiceOptions<BootNodeBlockApiService>(options =>
{
    options.MaxReceiveMessageSize = configurationOptions.GrpcMessageSizeLimit * 1024;
});

builder.Services.AddSingleton<core.IConfiguration>(configurationOptions);
builder.Services.AddSingleton<ILedgerIndexFactory, TypeFactory>();
builder.ConfigureStoreOptions();

if (false == configurationOptions.IsBootNode)
    builder.ConfigureChildNodeServices(configurationOptions);                
else
    builder.ConfigureBootNodeServices(configurationOptions);

builder.WebHost.ConfigureKestrel(kestrelOptions =>
{
    // Setup a HTTP/2 endpoint without TLS.  This is for listening for GRPC calls
    kestrelOptions.Listen(IPAddress.Any, configurationOptions.RPCPort, o =>
    {
        o.Protocols = HttpProtocols.Http2;
    });
    kestrelOptions.ConfigureHttpsDefaults(o =>
    {
        o.SslProtocols = SslProtocols.None;
    });
});

var app = builder.Build();

ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation($"Running as configured: {configurationOptions.ToString()}");

app.UseHealthChecks("/health");
app.UseRouting();

if (false == configurationOptions.IsBootNode)
    app.MapChildNodeGrpc();
else
    app.MapBootNodeGrpc();

app.FindAndStartPlugins();
app.StartLedger();

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical("Application is shutting down");
    app.StopPlugins();
    app.StopLedger();
});

app.Run();
