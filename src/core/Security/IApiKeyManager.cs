namespace core.Security;

/// <summary>
/// Limit access to the block chain by some value.  There are
/// 1 - write permissions
/// 2 - general read permissions
/// 3 - detailed read permissions
/// 4 - admin permissions
/// </summary>
public interface IApiKeyManager
{
    bool IsAdmin(string apiKey);
    bool IsWriteAllowed(string apiKey);
    bool IsReadAllowed(string apiKey);
    bool IsReadDetailsAllowed(string apiKey);
}