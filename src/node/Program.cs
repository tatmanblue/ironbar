
using System.Net;
using core.Ledger;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Node.General;
using Node.Ledger;
using Node.grpc.service;

namespace Node
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ConfigurationOptions configurationOptions = ConfigurationOptions.FromEnvironment();
            
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Logging.AddConsole();
            builder.Services.AddGrpc().AddServiceOptions<BootNodeBlockApiService>(options =>
            {
                // just a hard restriction on input specifically for creating blocks
                options.MaxReceiveMessageSize = configurationOptions.GrpcMessageSizeLimit * 1024;
            });
            
            builder.Services.AddSingleton<core.IConfiguration>(configurationOptions);
            builder.Services.AddSingleton<ILedgerIndexFactory, TypeFactory>();
            
            if (false == configurationOptions.IsBootNode)
                builder.ConfigureChildNodeServices(configurationOptions);                
            else
                builder.ConfigureBootNodeServices(configurationOptions);
            

            builder.WebHost.ConfigureKestrel(kestrelOptions =>
            {
                // TODO: should not use hardcoded Any.  Find more flexible options
                
                // Setup a HTTP/2 endpoint without TLS.  This is for listening for GRPC calls
                kestrelOptions.Listen(IPAddress.Any, configurationOptions.RPCPort, o => o.Protocols = HttpProtocols.Http2);
            });
            
            var app = builder.Build();
            
            ILogger<Program> l = app.Services.GetRequiredService<ILogger<Program>>();
            l.LogInformation($"Running as configured: {configurationOptions.ToString()}");

            app.UseRouting(); 

            if (false == configurationOptions.IsBootNode)
                app.MapChildNodeGrpc();
            else
                app.MapBootNodeGrpc();
            
            app.StartLedger();
            
            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopping.Register(() =>
            {
                ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogCritical("Application is shutting down");
                app.StopLedger();
            });
            
            
            app.Run();
        }
    }
}
