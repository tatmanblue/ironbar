using core.Security;
using core.Utility;

namespace Node.General;

/// <summary>
/// Limit access to the block chain by some value.  There are
/// 1 - write permissions
/// 2 - general read permissions
/// 3 - detailed read permissions
/// 4 - admin permissions
/// </summary>
public class ApiKeyManager : IApiKeyManager
{
    private string writeApiKey = string.Empty;
    private string readApiKey = string.Empty;
    private string readDetailsApiKey = string.Empty;
    private string adminApiKey = string.Empty;

    private string mcpWriteApiKey = string.Empty;
    private string mcpReadApiKey = string.Empty;
    private string mcpReadDetailsApiKey = string.Empty;
    private string mcpAdminApiKey = string.Empty;

    public ApiKeyManager()
    {
        writeApiKey       = EnvironmentUtility.FromEnvOrDefault("IRONBAR_WRITE_API_KEY", "");
        readApiKey        = EnvironmentUtility.FromEnvOrDefault("IRONBAR_READ_API_KEY", "");
        readDetailsApiKey = EnvironmentUtility.FromEnvOrDefault("IRONBAR_READ_DETAILS_API_KEY", "");
        adminApiKey       = EnvironmentUtility.FromEnvOrDefault("IRONBAR_ADMIN_API_KEY", "");

        mcpWriteApiKey       = EnvironmentUtility.FromEnvOrDefault("IRONBAR_MCP_WRITE_API_KEY", "");
        mcpReadApiKey        = EnvironmentUtility.FromEnvOrDefault("IRONBAR_MCP_READ_API_KEY", "");
        mcpReadDetailsApiKey = EnvironmentUtility.FromEnvOrDefault("IRONBAR_MCP_READ_DETAILS_API_KEY", "");
        mcpAdminApiKey       = EnvironmentUtility.FromEnvOrDefault("IRONBAR_MCP_ADMIN_API_KEY", "");
    }

    public bool IsAdmin(string apiKey)
    {
        if (string.IsNullOrEmpty(adminApiKey) && string.IsNullOrEmpty(mcpAdminApiKey))
            return false;

        if (0 == apiKey.CompareTo(adminApiKey) || 0 == apiKey.CompareTo(mcpAdminApiKey))
            return true;

        return false;
    }

    public bool IsWriteAllowed(string apiKey)
    {
        if (string.IsNullOrEmpty(writeApiKey) && string.IsNullOrEmpty(mcpWriteApiKey))
            return false;

        if (0 == apiKey.CompareTo(writeApiKey) || 0 == apiKey.CompareTo(mcpWriteApiKey))
            return true;

        return false;
    }

    public bool IsReadAllowed(string apiKey)
    {
        if (string.IsNullOrEmpty(readApiKey) && string.IsNullOrEmpty(mcpReadApiKey))
            return false;

        if (0 == apiKey.CompareTo(readApiKey) || 0 == apiKey.CompareTo(readDetailsApiKey) ||
            0 == apiKey.CompareTo(mcpReadApiKey) || 0 == apiKey.CompareTo(mcpReadDetailsApiKey))
            return true;

        return false;
    }

    public bool IsReadDetailsAllowed(string apiKey)
    {
        if (string.IsNullOrEmpty(readDetailsApiKey) && string.IsNullOrEmpty(mcpReadDetailsApiKey))
            return false;

        if (0 == apiKey.CompareTo(readDetailsApiKey) || 0 == apiKey.CompareTo(mcpReadDetailsApiKey))
            return true;

        return false;
    }

}