namespace core.Security
{
    /// <summary>
    /// Different systems will want to store their private keys differently.
    /// Examples are windows vs macos.  There could be other cases.  This
    /// interface is the contract those mechanisms must support so that
    /// a node can successfully manage keys 
    /// </summary>
    public interface IKeyStore
    {
    }
}
