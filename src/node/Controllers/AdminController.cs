using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Core.DTO;

namespace node.Controllers
{
    /**
     * This controller provides gateway with data and API for administration of the nodes
     */
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {

        private readonly ILogger<AdminController> logger;

        public AdminController(ILogger<AdminController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public List<ConnectedNode> Get()
        {
            List<ConnectedNode> nodes = new List<ConnectedNode>();
            nodes.Add(new ConnectedNode() { Name = "node1" });
            nodes.Add(new ConnectedNode() { Name = "node2" });
            return nodes;
        }
    }
}
