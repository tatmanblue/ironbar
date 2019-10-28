using System;
using CommandLine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace noderpc
{
    public class Options
    {
        [Option('r', HelpText = "Port to listen for RPC calls")]
        public int RPCPort { get; set; } = 5001;
        [Option('s', HelpText = "Server RPC call port")]
        public int ServerRPCPort { get; set; } = 5001;
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       // CreateHostBuilder(args, o).Build().Run();

                       IHostBuilder hostBuilder = CreateHostBuilder(args, o);
                       IHost host = hostBuilder.Build();

                       if (o.ServerRPCPort != o.RPCPort)
                       {
                           Console.WriteLine($"Connecting with {o.ServerRPCPort}");
                       }

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
                        // Setup a HTTP/2 endpoint without TLS.
                        options.ListenLocalhost(cmdOptions.RPCPort, o => o.Protocols =
                            HttpProtocols.Http2);
                    });
                    webBuilder.UseStartup<Startup>();
                });
        }
}