using Node.General;

namespace Node.Interfaces;

/// <summary>
/// 
/// </summary>
public interface IServicesEventSub
{
    /// <summary>
    /// Used by the boot node to handle any additional processing required
    /// when a node connects
    /// </summary>
    event ChildNodeConnected OnChildNodeConnected;
    /// <summary>
    /// Informs other services when a new block is created so the block can
    /// be processed
    /// </summary>
    event BlockCreated OnBlockCreated;
}