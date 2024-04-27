using System;
using System.Reflection;
using System.Text.Json.Serialization;
using core;
using core.Utility;
using IConfiguration = core.IConfiguration;

namespace Node.General;

public class ConfigurationOptions : IConfiguration
{
    /// <summary>
    /// this is the friendly name that appears in logs etc....
    /// </summary>
    public string FriendlyName { get; set; } = "Node";

    public string Version { get; } = "0.0.3.5";
    /// <summary>
    /// This node, boot or child, will listen for RPC calls on this port
    /// </summary>
    public int RPCPort { get; set; } = 50051;

    /// <summary>
    /// Only important when IsBootNode == false, indicates how to locate the
    /// boot node
    /// </summary>
    public string BootAddress { get; set; } = "http://localhost:50051";
    
    public string PluginPath { get; set; } = "plugins";
    public string DataPath { get; set; } = "data";
    public bool IsBootNode { get; set; } = false;
    /// <summary>
    /// in KB. thus 1 is approximately 1K
    /// this limit is put in place because deployment might be expensive 
    /// </summary>
    public int GrpcMessageSizeLimit { get; set; } = 1;
    
    [JsonConstructor]
    public ConfigurationOptions() {}

    public override string ToString()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        Version version = assembly.GetName().Version;

        // Print the version to the console
        Console.WriteLine("Assembly Version: " + version);        
        
        return $"\r\n" +
               $"IsBootNode: {IsBootNode}\r\n" +
               $"RPC Server: {BootAddress}\r\n" +
               $"RPC Port: {RPCPort}\r\n" +
               $"Version: {Version}\r\n" +
               $"Assembly Version: {version}\r\n"
            ;
    }

    public static ConfigurationOptions FromEnvironment()
    {
        ConfigurationOptions options = new ConfigurationOptions();

        if ("boot" == FromEnvOrDefault("IRONBAR_TYPE", "client"))
        {
            options.IsBootNode = true;
            options.FriendlyName = "boot";
        }
        else
        {
            options.FriendlyName = FromEnvOrDefault("IRONBAR_NODE_NAME", "node");
        }

        options.RPCPort = FromEnvOrDefaultAsInt("IRONBAR_RPC_PORT", "50051");
        options.BootAddress = FromEnvOrDefault("IRONBAR_BOOT_SERVER", "http://localhost:50051");
        options.DataPath = FromEnvOrDefault("IRONBAR_DATA_PATH", "data");
        options.PluginPath = FromEnvOrDefault("IRONBAR_PLUGIN_PATH", "plugins");
        options.GrpcMessageSizeLimit = FromEnvOrDefaultAsInt("IRONBAR_GRPC_LIMIT", "1");
        
        return options;
    }

    /// <summary>
    /// I left these here to make FromEnvironment() a weeeee bit less wordy
    /// </summary>
    private static string FromEnvOrDefault(string name, string def)
    {
        return EnvironmentUtility.FromEnvOrDefault(name, def);
    }
    
    /// <summary>
    /// I left these here to make FromEnvironment() a weeeee bit less wordy
    /// </summary>
    private static int FromEnvOrDefaultAsInt(string name, string def)
    {
        return EnvironmentUtility.FromEnvOrDefaultAsInt(name, def);
    }
}

