namespace Node.grpc.service;

#region Exceptions
public class ChildNodeServiceException : Exception
{
    public ChildNodeServiceException(string message) : base(message) { }
}
#endregion

#region enums
public enum ChildNodeServiceState
{
    NotStarted,
    ConnectingToBootNode,
    Running,
    ShuttingDown,
    Halted,
    RetryingConnectionToBootNode,
}
#endregion