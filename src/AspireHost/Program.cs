using DotNetEnv;
using Projects;

string GetEnvVar(string varName, string defaultValue = "")
{
    string? value = Environment.GetEnvironmentVariable(varName);
    if (value is null)
    {
        Console.WriteLine($"Environment variable '{varName}' is not set. Using default value '{defaultValue}'.");
        return defaultValue;
    }
    return value;
}

var builder = DistributedApplication.CreateBuilder(args);
string type = GetEnvVar("IRONBAR_TYPE", "boot");
string rpcPort = GetEnvVar("IRONBAR_RPC_PORT", "50051");
string dataPath = GetEnvVar("IRONBAR_DATA_PATH", "data");
string pluginPath = GetEnvVar("IRONBAR_PLUGIN_PATH", "plugins");
string grpcLimit = GetEnvVar("IRONBAR_GRPC_LIMIT", "1");
string svcUri = GetEnvVar("IRONBAR_SVC_URI", "");

// configure the bootnode
builder.AddProject<node>("BootNode")
    .WithEnvironment("IRONBAR_TYPE", type)
    .WithEnvironment("IRONBAR_RPC_PORT", rpcPort)
    .WithEnvironment("IRONBAR_DATA_PATH", dataPath)
    .WithEnvironment("IRONBAR_PLUGIN_PATH", pluginPath)
    .WithEnvironment("IRONBAR_GRPC_LIMIT", grpcLimit)
    .WithEnvironment("IRONBAR_SVC_URI", svcUri);

builder.Build().Run();
