namespace Node.General;

/// <summary>
/// Limit access to the block chain by some value.  There are
/// 1 - write permissions
/// 2 - general read permissions
/// 3 - detailed read permissions
///
/// TODO prob should extract an interface and make consumers work against an interface so that
/// TODO this can be extended at some point
/// </summary>
public class ApiKeyManager
{
    private string writeApiKey = string.Empty;
    private string readApiKey = string.Empty;
    private string readDetailsApiKey = string.Empty;
    private string adminApiKey = string.Empty;

    public ApiKeyManager()
    {
        writeApiKey = FromEnvOrDefault("IRONBAR_WRITE_API_KEY", "");
        readApiKey = FromEnvOrDefault("IRONBAR_READ_API_KEY", "");
        readDetailsApiKey = FromEnvOrDefault("IRONBAR_READ_DETAILS_API_KEY", "");
        adminApiKey = FromEnvOrDefault("IRONBAR_ADMIN_API_KEY", "");
    }

    public bool IsAdmin(string apiKey)
    {
        if (string.IsNullOrEmpty(adminApiKey))
            return false;
        
        if (0 == apiKey.CompareTo(adminApiKey))
            return true;
        
        return false;
    }
    
    public bool IsWriteAllowed(string apiKey)
    {
        if (string.IsNullOrEmpty(writeApiKey))
            return false;

        if (0 == apiKey.CompareTo(writeApiKey))
            return true;
        
        return false;
    }

    public bool IsReadAllowed(string apiKey)
    {
        if (string.IsNullOrEmpty(readApiKey))
            return false;

        if (0 == apiKey.CompareTo(readApiKey) || 0 == apiKey.CompareTo(readDetailsApiKey))
            return true;
        
        return false;
    }
    
    public bool IsReadDetailsAllowed(string apiKey)
    {
        if (string.IsNullOrEmpty(readDetailsApiKey))
            return false;

        if (0 == apiKey.CompareTo(readDetailsApiKey))
            return true;
        
        return false;
    }
    
    private static string FromEnvOrDefault(string name, string def)
    {
        string v = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrEmpty(v))
            return def;

        return v;
    }
}