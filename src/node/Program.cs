using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CommandLine;
using node.Services;
using node.General;
using core;
using core.Utility;

namespace node
{

    public class SwipeData
    {
    }

    public class Program
    {
        public static event Action<SwipeData> OnSwipe = delegate { };
        
        public static void Main(string[] args)
        {
            Program.OnSwipe += ProgramOnOnSwipe; 
            
            // only one command line argument is allowed and that is to the configuration file
            // it can be relative or literal path 
            if (1 != args.Length)
                throw new GeneralException("command line must specific configuration file");

            Configuration options = JsonUtility.DeserializeFromFile<Configuration>(args[0]);

            IHostBuilder hostBuilder = CreateHostBuilder(args, options);
            IHost host = hostBuilder.Build();

            host.Run();
        }

        private static void ProgramOnOnSwipe(SwipeData obj)
        {
            throw new NotImplementedException();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args, Configuration configuration) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddConfiguration(context.Configuration.GetSection("Logging"));

                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        logging.AddConsole();
                        logging.AddDebug();
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    Console.WriteLine($"RPCPort configured to use {configuration.RPCPort}");
                    webBuilder.ConfigureKestrel(options =>
                    {
                        // Setup a HTTP/2 endpoint without TLS.  This is for listening for GRPC calls
                        options.ListenLocalhost(configuration.RPCPort, o => o.Protocols = HttpProtocols.Http2);

                        // set up webservices port
                        if (true == configuration.IsBootNode)
                            options.ListenLocalhost(configuration.APIPort, o => o.Protocols = HttpProtocols.Http1);
                    });
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    // initialization for all node types
                    services.AddSingleton<IConfiguration>(configuration);

                    if (false == configuration.IsBootNode)
                    {
                        // initialization for child nodes
                        services.AddTransient<ChildNodeRPCClient>();
                        services.AddHostedService<ChildNodeService>();
                    }
                    else
                    {
                        // initialization for boot node
                        services.AddSingleton<ConnectionManager>();
                        services.AddTransient<BootNodeRPCClient>();
                    }
                });
        }
}
