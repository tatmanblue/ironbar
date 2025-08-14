using core.Ledger;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Node.grpc.service;
using Node.Interfaces;
using Node.Ledger;
using storage;

namespace Node.General;

public static class NodeExtensions
{
    /// <summary>
    /// Creates the appropriate ledger reader/writer based on configuration and registers them
    /// in the DI container
    /// </summary>
    /// <param name="builder"></param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ApplicationException"></exception>
    public static void ConfigureStoreOptions(this WebApplicationBuilder builder)
    {
        core.IConfiguration options = builder.Services.BuildServiceProvider().GetService<core.IConfiguration>();
        ILedgerReader reader;
        ILedgerWriter writer;
        ILedgerIndexFactory indexFactory;
        ILedgerPhysicalBlockFactory blockFactory;

        if (options.StorageType == StorageType.AzureBlob)
        {

            // may wont to consider moving this to the configuration options
            string accountName = Environment.GetEnvironmentVariable("IRONBAR_AZURE_BLOB_ACCOUNT_NAME") ?? 
                                 throw new ApplicationException("IRONBAR_AZURE_BLOB_ACCOUNT_NAME environment variable not set");
            string accountKey = Environment.GetEnvironmentVariable("IRONBAR_AZURE_BLOB_ACCOUNT_KEY") ?? 
                                throw new ApplicationException("IRONBAR_AZURE_BLOB_ACCOUNT_KEY environment variable not set");
            
            ILogger<AzureBlobReaderWriter> logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<AzureBlobReaderWriter>>();
            reader = new AzureBlobReaderWriter(logger, options.FriendlyName,  accountName, accountKey);
            writer = reader as ILedgerWriter ?? throw new InvalidOperationException();
            indexFactory = new JsonLedgerIndexTypeFactory();
            blockFactory = new JsonPhysicalBlockTypeFactory();
        }
        else
        {
            string ledgerPath = Path.Combine(options.DataPath, options.FriendlyName);
            reader = new DefaultTextFileLedgerReader(ledgerPath);
            writer = new DefaultTextFileLedgerWriter(ledgerPath);
            indexFactory = new TextFileLedgerIndexTypeFactory();
            blockFactory = new TextFilePhysicalBlockTypeFactory();
        }
        
        builder.Services.AddSingleton<ILedgerReader>(reader);
        builder.Services.AddSingleton<ILedgerWriter>(writer);
        builder.Services.AddSingleton<ILedgerIndexFactory>(indexFactory);
        builder.Services.AddSingleton<ILedgerPhysicalBlockFactory>(blockFactory);
    }
    
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