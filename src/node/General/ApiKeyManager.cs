namespace Node.General;

/// <summary>
/// Limit access to the block chain by some value
/// </summary>
public class ApiKeyManager
{
    private string writeApiKey = string.Empty;
    private string readApiKey = string.Empty;
    private string readDetailsApiKey = string.Empty;

    public ApiKeyManager()
    {
        writeApiKey = FromEnvOrDefault("IRONBAR_WRITE_API_KEY", "");
        readApiKey = FromEnvOrDefault("IRONBAR_READ_API_KEY", "");
        readDetailsApiKey = FromEnvOrDefault("IRONBAR_READ_DETAILS_API_KEY", "");
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