using Node.grpc.service;
using Node.Interfaces;
using Node.Ledger;

namespace Node.General;

public static class NodeExtensions
{
    public static void ConfigureBootNodeServices(this WebApplicationBuilder builder, core.IConfiguration options)
    {
        if (options.IsBootNode == false)
            throw new ApplicationException($"Wrong node configuration called bootNode={options.IsBootNode}");
        
        builder.Services.AddSingleton<BootNodeServicesEvents>();
        builder.Services.AddSingleton<IServicesEventPub>(x => x.GetRequiredService<BootNodeServicesEvents>());
        builder.Services.AddSingleton<IServicesEventSub>(x => x.GetRequiredService<BootNodeServicesEvents>());

        // initialization for boot node
        builder.Services.AddTransient<ApiKeyManager>();
        builder.Services.AddSingleton<ConnectionManager>();
        builder.Services.AddTransient<BootNodeRPCClient>();
    }
    
    public static void ConfigureClientNodeServices(this WebApplicationBuilder builder, core.IConfiguration options)
    {
        if (options.IsBootNode == true)
            throw new ApplicationException($"Wrong node configuration called bootNode={options.IsBootNode}");

        builder.Services.AddSingleton<ClientNodeServicesEvents>();
        builder.Services.AddSingleton<IServicesEventPub>(x => x.GetRequiredService<ClientNodeServicesEvents>());
        builder.Services.AddSingleton<IServicesEventSub>(x => x.GetRequiredService<ClientNodeServicesEvents>());
                
                
        // initialization for child nodes
        builder.Services.AddTransient<ChildNodeRPCClient>();
        builder.Services.AddHostedService<ChildNodeService>();
    }
}