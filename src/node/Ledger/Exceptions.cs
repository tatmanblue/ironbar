using Grpc.Core;

namespace Node.Ledger;

/// <summary>
/// thrown when api manager returns false == IsApiKeyAllowed
/// </summary>
public class ApiKeyManagerException : RpcException 
{
    public ApiKeyManagerException(string msg = "") : base(new Status(StatusCode.PermissionDenied, msg)) {}
}