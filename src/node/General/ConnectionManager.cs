using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Node.grpc.service;

namespace Node.General;

/// <summary>
/// Data about a child node.  It will be expanded as there is more data needed
/// </summary>
public class ChildNodeConnection
{
    public string Address { get; set; }
}

/// <summary>
/// Used by boot node, its responsible for maintaining data on child nodes connected in
/// to boot node (aka the network).  Maintains:
/// 1) child node states
/// 2) child node activity (later work such as when validating a new ledger block etc)
/// </summary>
public class ConnectionManager
{
    private readonly List<ChildNodeConnection> _children = new List<ChildNodeConnection>();
    private readonly ILogger<ConnectionManager> _logger;
    private readonly BootNodeRPCClient _client;

    public ConnectionManager(BootNodeRPCClient client, IHostApplicationLifetime lifeTime, ILogger<ConnectionManager> logger)
    {
        _client = client;
        _logger = logger;

        lifeTime.ApplicationStopping.Register(() => {
            HandleServiceShutdown();
        });
    }

    public void AddNewChildNode(ChildNodeConnection child)
    {
        _children.Add(child);
    }

    public void RemoveChildNode(string address)
    {
        ChildNodeConnection child = _children.Find(c => c.Address == address);
        _children.Remove(child);
    }

    public void HandleServiceShutdown()
    {
        _logger.LogInformation("ConnectionManager handling application shut down");
        foreach(ChildNodeConnection child in _children)
        {
            // ok to not await this call as we want to shutdown messages sent as quickly as possible.  
            // errors should not affect continuation.  we are in shutdown and its more important
            // this completes as fast as possible
#pragma warning disable 4014
            _client.SendShuttingDownMessage(child.Address);
#pragma warning restore 4014
        }
    }
}
