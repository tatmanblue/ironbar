using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CommandLine;
using node.Services;
using node.General;

namespace node
{


    public class Program
    {
        // TODO: would like to make this injectable vs static instance
        public static Options SystemOptions { get; private set; } = null;

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       Program.SystemOptions = o;

                       IHostBuilder hostBuilder = CreateHostBuilder(args, o);
                       IHost host = hostBuilder.Build();

                       host.Run();
                   });
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args, Options cmdOptions) =>
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
                    Console.WriteLine($"RPCPort configured to use {cmdOptions.RPCPort}");
                    webBuilder.ConfigureKestrel(options =>
                    {
                        // Setup a HTTP/2 endpoint without TLS.  This is for listening for GRPC calls
                        options.ListenLocalhost(cmdOptions.RPCPort, o => o.Protocols = HttpProtocols.Http2);

                        // set up webservices port
                        if (true == cmdOptions.IsBootNode)
                            options.ListenLocalhost(cmdOptions.APIPort, o => o.Protocols = HttpProtocols.Http1);
                    });
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    // initialization for all node types
                    services.AddSingleton<IOptions>(Program.SystemOptions);

                    if (false == cmdOptions.IsBootNode)
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
