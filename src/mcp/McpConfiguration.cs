namespace IronBar.MCP;

public sealed class McpConfiguration
{
    public string BootNodeAddress { get; init; }
    public string ReadApiKey { get; init; }
    public string ReadDetailsApiKey { get; init; }
    public string WriteApiKey { get; init; }
    public string AdminApiKey { get; init; }

    /// <summary>
    /// Set of tokens that agents must supply in the Authorization: Bearer header to invoke
    /// protected MCP tools.  Populated from IRONBAR_MCP_ACCESS_TOKENS (comma-separated).
    /// An empty set means no enforcement — all requests are allowed (development mode).
    /// </summary>
    public IReadOnlySet<string> AccessTokens { get; init; }

    private McpConfiguration(
        string bootNodeAddress,
        string readApiKey,
        string readDetailsApiKey,
        string writeApiKey,
        string adminApiKey,
        IReadOnlySet<string> accessTokens)
    {
        BootNodeAddress = bootNodeAddress;
        ReadApiKey = readApiKey;
        ReadDetailsApiKey = readDetailsApiKey;
        WriteApiKey = writeApiKey;
        AdminApiKey = adminApiKey;
        AccessTokens = accessTokens;
    }

    public static McpConfiguration FromEnvironment() =>
        new(
            bootNodeAddress: FromEnv("IRONBAR_BOOTNODE_ADDRESS", "http://localhost:50051"),
            readApiKey: FromEnv("IRONBAR_MCP_READ_API_KEY", ""),
            readDetailsApiKey: FromEnv("IRONBAR_MCP_READ_DETAILS_API_KEY", ""),
            writeApiKey: FromEnv("IRONBAR_MCP_WRITE_API_KEY", ""),
            adminApiKey: FromEnv("IRONBAR_MCP_ADMIN_API_KEY", ""),
            accessTokens: ParseTokens(FromEnv("IRONBAR_MCP_ACCESS_TOKENS", ""))
        );

    private static string FromEnv(string name, string defaultValue) =>
        Environment.GetEnvironmentVariable(name) ?? defaultValue;

    private static IReadOnlySet<string> ParseTokens(string value) =>
        value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
             .ToHashSet();
}
