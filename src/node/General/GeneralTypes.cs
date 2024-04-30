namespace Node.General;

public enum ChildNodeState
{
    Unknown,
    Requested,          // child node sent "Connect" request to boot node
    Initializing,       // child node requested the index
    Allowed,            // child node validated the indexes and has/will validate blocks
    Denied              // some type of condition prevents child node from participating
}

/// <summary>
/// Data about a child node.  It will be expanded as there is more data needed
/// </summary>
public class ChildNodeConnection
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string Address { get; set; }
    public ChildNodeState State { get; set; }
}

public static class Globals
{
    public static string APPROVED = "true";
}

public class ConnectionManagerException : Exception
{
    
}