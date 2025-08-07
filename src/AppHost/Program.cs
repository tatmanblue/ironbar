using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// configure the bootnode
builder.AddProject<node>("BootNode")
    .WithEnvironment("IRONBAR_TYPE", "boot");
    
builder.Build().Run();
