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


Env.Load();

var builder = DistributedApplication.CreateBuilder(args);
string type = GetEnvVar("IRONBAR_TYPE", "boot");
string rpcPort = GetEnvVar("IRONBAR_RPC_PORT", "50051");
string bootAddress = GetEnvVar("IRONBAR_BOOT_SERVER", "http://localhost:50051");
string dataPath = GetEnvVar("IRONBAR_DATA_PATH", "data");
string pluginPath = GetEnvVar("IRONBAR_PLUGIN_PATH", "plugins");
string grpcLimit = GetEnvVar("IRONBAR_GRPC_LIMIT", "1");
string svcUri = GetEnvVar("IRONBAR_SVC_URI", "");
string storage = GetEnvVar("IRONBAR_STORAGE_TYPE", "filesystem");
string writeApiKey = GetEnvVar("IRONBAR_WRITE_API_KEY", "");
string readApiKey = GetEnvVar("IRONBAR_READ_API_KEY", "");
string readDetailApiKey = GetEnvVar("IRONBAR_READ_DETAIL_API_KEY", "");
string adminApiKey = GetEnvVar("IRONBAR_ADMIN_API_KEY", "");

// configure the bootnode
var node = builder.AddProject<node>("BootNode")
    .WithEnvironment("IRONBAR_TYPE", type)
    .WithEnvironment("IRONBAR_RPC_PORT", rpcPort)
    .WithEnvironment("IRONBAR_DATA_PATH", dataPath)
    .WithEnvironment("IRONBAR_BOOT_SERVER", bootAddress)
    .WithEnvironment("IRONBAR_PLUGIN_PATH", pluginPath)
    .WithEnvironment("IRONBAR_GRPC_LIMIT", grpcLimit)
    .WithEnvironment("IRONBAR_SVC_URI", svcUri)
    .WithEnvironment("IRONBAR_STORAGE_TYPE", storage)
    .WithEnvironment("IRONBAR_WRITE_API_KEY", writeApiKey)
    .WithEnvironment("IRONBAR_READ_API_KEY", readApiKey)
    .WithEnvironment("IRONBAR_READ_DETAIL_API_KEY", readDetailApiKey)
    .WithEnvironment("IRONBAR_ADMIN_API_KEY", adminApiKey);


if (storage == "azureblob")
{
    string accountName = GetEnvVar("IRONBAR_AZURE_BLOB_ACCOUNT_NAME", "");
    string accountKey = GetEnvVar("IRONBAR_AZURE_BLOB_ACCOUNT_KEY", "");
    if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(accountKey))
    {
        throw new ApplicationException("Azure Blob storage selected but account name or key is not set.");
    }
    node = node
        .WithEnvironment("IRONBAR_AZURE_BLOB_ACCOUNT_NAME", accountName)
        .WithEnvironment("IRONBAR_AZURE_BLOB_ACCOUNT_KEY", accountKey);
}

builder.Build().Run();
