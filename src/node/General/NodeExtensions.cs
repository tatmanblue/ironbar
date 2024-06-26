﻿using core.Ledger;
using Microsoft.Extensions.Diagnostics.HealthChecks;
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
        
        // For now, all nodes have ledger manager,  there might be some differences in behavior
        // between a bootnode ledger manager and a child nodes ledger manager that will
        // make us want to split this out
        builder.Services.AddSingleton<ILedgerManager, LedgerManager>();
        
        builder.Services.AddSingleton<BootNodeServicesEvents>();
        builder.Services.AddSingleton<IServicesEventPub>(x => x.GetRequiredService<BootNodeServicesEvents>());
        builder.Services.AddSingleton<IServicesEventSub>(x => x.GetRequiredService<BootNodeServicesEvents>());
        
        builder.Services.AddSingleton<IPhysicalBlockValidator>(x => new BootNodePhysicalBlockValidator());                

        
        // initialization for boot node
        builder.Services.AddTransient<ApiKeyManager>();
        builder.Services.AddSingleton<ConnectionManager>();
        builder.Services.AddHostedService<BootNodeRPCClient>();
    }
    
    public static void ConfigureChildNodeServices(this WebApplicationBuilder builder, core.IConfiguration options)
    {
        if (options.IsBootNode == true)
            throw new ApplicationException($"Wrong node configuration called bootNode={options.IsBootNode}");
        
        // For the moment, to "fix" differences between bootnode ledger start up and client node
        builder.Services.AddSingleton<ILedgerManager, ChildLedgerManager>();
        
        builder.Services.AddSingleton<ChildNodeServicesEvents>();
        builder.Services.AddSingleton<IServicesEventPub>(x => x.GetRequiredService<ChildNodeServicesEvents>());
        builder.Services.AddSingleton<IServicesEventSub>(x => x.GetRequiredService<ChildNodeServicesEvents>());

        builder.Services.AddSingleton<ChildPhysicalBlockValidator>();
        builder.Services.AddSingleton<IPhysicalBlockValidator>(x => new ChildPhysicalBlockValidator());                
                
        // initialization for child nodes
        builder.Services.AddTransient<ChildNodeRPCClient>();
        builder.Services.AddHostedService<ChildNodeService>();
    }

    public static void MapBootNodeGrpc(this WebApplication app)
    {
        ILogger<Program> logger = app.Services.GetRequiredService< ILogger<Program>>();
        core.IConfiguration options = app.Services.GetRequiredService<core.IConfiguration>();
        if (false == options.IsBootNode)
        {
            logger.LogDebug("Not setting up bootnode GRPC end points");
            return;
        }

        logger.LogInformation("Setting up boot node GRPC endpoints");
        app.UseEndpoints(endpoints =>
        {
              endpoints.MapGrpcService<BootNodeRPCService>();
              endpoints.MapGrpcService<BootNodeBlockApiService>();
              endpoints.MapHealthChecks("/health");

        });
    }
    
    public static void MapChildNodeGrpc(this WebApplication app)
    {
        ILogger<Program> logger = app.Services.GetRequiredService< ILogger<Program>>();
        core.IConfiguration options = app.Services.GetRequiredService<core.IConfiguration>();
        if (true == options.IsBootNode)
        {
            logger.LogDebug("Not setting up child node GRPC end points");
            return;
        }

        logger.LogInformation("Setting up child node GRPC endpoints");
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<ChildNodeService>();
            endpoints.MapGrpcService<ChildNodeRPCService>();
            endpoints.MapHealthChecks("/health");
        });
    }
}