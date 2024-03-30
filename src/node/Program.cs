
using core.Utility;
using Node.General;
using Node.Ledger;

namespace nodev2
{
    using Node.grpc.service;

    public class Program
    {
        public static void Main(string[] args)
        {

            ConfigurationOptions configurationOptions = new ConfigurationOptions();
            // only one command line argument is allowed and that is to the configuration file
            // it can be relative or literal path 
            if (1 == args.Length)
                configurationOptions = JsonUtility.DeserializeFromFile<ConfigurationOptions>(args[0]);
            
            
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.AddConsole(); // Add Console logging provider
            builder.Services.AddGrpc();
            
            builder.Services.AddSingleton<core.IConfiguration>(configurationOptions);
            // For now, all nodes have ledger manager,  there might be some differences in behavior
            // between a bootnode ledger manager and a child nodes ledger manager that will
            // make us want to split this out
            builder.Services.AddSingleton<ILedgerManager, LedgerManager>();
            if (false == configurationOptions.IsBootNode)
            {
                // initialization for child nodes
                builder.Services.AddTransient<ChildNodeRPCClient>();
                builder.Services.AddHostedService<ChildNodeService>();
            }
            else
            {
                // initialization for boot node
                builder.Services.AddSingleton<ConnectionManager>();
                builder.Services.AddTransient<BootNodeRPCClient>();
            }
            
            var app = builder.Build();

            app.UseRouting(); // Add this line to enable routing

            app.MapGet("/", () => "Hello World!");
            app.UseEndpoints(endpoints =>
            {
                if (configurationOptions.IsBootNode)
                    endpoints.MapGrpcService<BootNodeRPCService>();
                else
                    endpoints.MapGrpcService<ChildNodeService>();
            });

            
            app.StartLedger();
            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopping.Register(() =>
            {
                ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogCritical("Application is shutting down");
            });
            
            
            app.Run();
        }
    }
}
