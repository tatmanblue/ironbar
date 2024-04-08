namespace Node.General;

/// <summary>
/// Limit access to the block chain by some value
/// </summary>
public class ApiKeyManager
{
    /// <summary>
    /// TODO temporary implementation
    /// </summary>
    /// <param name="apiKey"></param>
    /// <returns></returns>
    public bool IsApiKeyAllowed(string apiKey)
    {
        return true;
    }
}