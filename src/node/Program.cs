
using System.Net;
using core.Ledger;
using core.Utility;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Node.General;
using Node.Interfaces;
using Node.Ledger;

namespace Node
{
    using Node.grpc.service;

    public class Program
    {
        public static void Main(string[] args)
        {

            BootNodeServicesEvents bootNodeServicesEvents;
            ConfigurationOptions configurationOptions = ConfigurationOptions.FromEnvironment();
            
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Logging.AddConsole();
            builder.Services.AddGrpc().AddServiceOptions<BootNodeBlockApiService>(options =>
            {
                // just a hard restriction on input specifically for creating blocks
                options.MaxReceiveMessageSize = configurationOptions.GrpcMessageSizeLimit * 1024;
            });
            
            builder.Services.AddSingleton<core.IConfiguration>(configurationOptions);
            
            // For now, all nodes have ledger manager,  there might be some differences in behavior
            // between a bootnode ledger manager and a child nodes ledger manager that will
            // make us want to split this out
            // builder.Services.AddSingleton<ILedgerManager, LedgerManager>();
            builder.Services.AddSingleton<ILedgerIndexFactory, TypeFactory>();
            
            if (false == configurationOptions.IsBootNode)
                builder.ConfigureClientNodeServices(configurationOptions);                
            else
                builder.ConfigureBootNodeServices(configurationOptions);
            

            builder.WebHost.ConfigureKestrel(kestrelOptions =>
            {
                // TODO: should not use hardcoded Any.  Find more flexible options
                
                // Setup a HTTP/2 endpoint without TLS.  This is for listening for GRPC calls
                kestrelOptions.Listen(IPAddress.Any, configurationOptions.RPCPort, o => o.Protocols = HttpProtocols.Http2);

                /*
                // set up webservices port
                if (true == configurationOptions.IsBootNode)
                    kestrelOptions.Listen(IPAddress.Any, configurationOptions.APIPort, o => o.Protocols = HttpProtocols.Http1AndHttp2);
                */                    
            });
            
            var app = builder.Build();
            
            ILogger<Program> l = app.Services.GetRequiredService<ILogger<Program>>();
            l.LogInformation($"Running as configured: {configurationOptions.ToString()}");

            app.UseRouting(); // Add this line to enable routing
            app.UseEndpoints(endpoints =>
            {
                if (configurationOptions.IsBootNode)
                {
                    endpoints.MapGrpcService<BootNodeRPCService>();
                    endpoints.MapGrpcService<BootNodeBlockApiService>();
                }
                else
                {
                    endpoints.MapGrpcService<ChildNodeService>();
                    endpoints.MapGrpcService<ChildNodeRPCService>();
                }
            });
            
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
