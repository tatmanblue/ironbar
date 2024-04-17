using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Node.grpc.service;
using Node.Interfaces;

namespace Node.General;

/// <summary>
/// Used by boot node, its responsible for maintaining data on child nodes connected in
/// to boot node (aka the network).  Maintains:
/// 1) child node states
/// 2) child node activity (later work such as when validating a new ledger block etc)
/// </summary>
public class ConnectionManager
{
    private readonly List<ChildNodeConnection> children = new List<ChildNodeConnection>();
    private readonly ILogger<ConnectionManager> logger;
    private readonly BootNodeRPCClient client;
    private readonly IServicesEventPub servicesEventPub;

    /// <summary>
    /// TODO There is an ugly circular dependency with BootNodeRPCClient
    /// </summary>
    /// <param name="client"></param>
    /// <param name="lifeTime"></param>
    /// <param name="logger"></param>
    public ConnectionManager(BootNodeRPCClient client, 
        IHostApplicationLifetime lifeTime, 
        ILogger<ConnectionManager> logger,
        IServicesEventPub servicesEventPub)
    {
        this.client = client;
        this.logger = logger;
        this.servicesEventPub = servicesEventPub;

        lifeTime.ApplicationStopping.Register(() => {
            HandleServiceShutdown();
        });
    }

    public void AddNewChildNode(ChildNodeConnection child)
    {
        children.Add(child);
    }

    public void RemoveChildNode(string address)
    {
        ChildNodeConnection child = children.Find(c => c.Address == address);
        children.Remove(child);
    }

    public void HandleServiceShutdown()
    {
        logger.LogInformation("ConnectionManager handling application shut down");
        foreach(ChildNodeConnection child in children)
        {
            servicesEventPub.FireServiceShutdown(child);
        }
    }
}
