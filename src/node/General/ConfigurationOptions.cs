using System;
using System.Text.Json.Serialization;
using core;
using IConfiguration = core.IConfiguration;

namespace Node.General;

public class ConfigurationOptions : IConfiguration
{
    /// <summary>
    /// this is the friendly name that appears in logs etc....
    /// </summary>
    public string FriendlyName { get; set; } = "Node";
    /// <summary>
    /// This node, boot or child, will listen for RPC calls on this port
    /// </summary>
    public int RPCPort { get; set; } = 50051;

    /// <summary>
    /// Only important when IsBootNode == false, indicates how to locate the
    /// boot node
    /// </summary>
    public string BootAddress { get; set; } = "http://localhost:50051";
    
    /*
    public string RPCAddress { get; set; } = "0.0.0.0";
    public int ServerRPCPort { get; set; } = 50051;
    public int APIPort { get; set; } = 8080;
    */
    public string PluginPath { get; set; } = "plugins";
    public string DataPath { get; set; } = "data";
    public bool IsBootNode { get; set; } = false;
    
    [JsonConstructor]
    public ConfigurationOptions() {}

    public override string ToString()
    {
        return $"\r\n" +
               $"IsBootNode: {IsBootNode}\r\n" +
               $"RPC Server: {BootAddress}\r\n" +
               $"RPC Port: {RPCPort}\r\n"
            ;
    }

    public static ConfigurationOptions FromEnvironment()
    {
        ConfigurationOptions options = new ConfigurationOptions();

        if ("boot" == FromEnvOrDefault("IRONBAR_TYPE", "client"))
            options.IsBootNode = true;

        options.RPCPort = FromEnvOrDefaultAsInt("IRONBAR_RPC_PORT", "50051");
        options.BootAddress = FromEnvOrDefault("IRONBAR_BOOT_SERVER", "http://localhost:50051");
        options.DataPath = FromEnvOrDefault("IRONBAR_DATA_PATH", "data");
        options.PluginPath = FromEnvOrDefault("IRONBAR_PLUGIN_PATH", "plugins");
        
        return options;
    }

    private static string FromEnvOrDefault(string name, string def)
    {
        string v = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrEmpty(v))
            return def;

        return v;
    }
    
    private static int FromEnvOrDefaultAsInt(string name, string def)
    {
        return Convert.ToInt32(FromEnvOrDefault(name, def));
    }
}

