namespace IronBar.MCP;

public sealed class McpConfiguration
{
    public string BootNodeAddress { get; init; }
    public string ReadApiKey { get; init; }
    public string ReadDetailsApiKey { get; init; }
    public string WriteApiKey { get; init; }
    public string AdminApiKey { get; init; }

    private McpConfiguration(
        string bootNodeAddress,
        string readApiKey,
        string readDetailsApiKey,
        string writeApiKey,
        string adminApiKey)
    {
        BootNodeAddress = bootNodeAddress;
        ReadApiKey = readApiKey;
        ReadDetailsApiKey = readDetailsApiKey;
        WriteApiKey = writeApiKey;
        AdminApiKey = adminApiKey;
    }

    public static McpConfiguration FromEnvironment() =>
        new(
            bootNodeAddress: FromEnv("IRONBAR_BOOTNODE_ADDRESS", "http://localhost:50051"),
            readApiKey: FromEnv("IRONBAR_MCP_READ_API_KEY", ""),
            readDetailsApiKey: FromEnv("IRONBAR_MCP_READ_DETAILS_API_KEY", ""),
            writeApiKey: FromEnv("IRONBAR_MCP_WRITE_API_KEY", ""),
            adminApiKey: FromEnv("IRONBAR_MCP_ADMIN_API_KEY", "")
        );

    private static string FromEnv(string name, string defaultValue) =>
        Environment.GetEnvironmentVariable(name) ?? defaultValue;
}
