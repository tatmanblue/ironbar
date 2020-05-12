using Grpc.Core.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using node.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace node.General
{
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
        private List<ChildNodeConnection> _children = new List<ChildNodeConnection>();
        private ILogger<ConnectionManager> _logger;
        private BootNodeClient _client;

        public ConnectionManager(BootNodeClient client, IHostApplicationLifetime lifeTime, ILogger<ConnectionManager> logger)
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

        public void HandleServiceShutdown()
        {
            _logger.LogInformation("ConnectionManager handling application shut down");
            foreach(ChildNodeConnection child in _children)
            {
                _client.SendShuttingDownMessage(child.Address);
            }
        }
    }
}
