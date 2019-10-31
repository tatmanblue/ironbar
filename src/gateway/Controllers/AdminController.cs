using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace noderpc.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {

        private readonly ILogger<AdminController> logger;

        public AdminController(ILogger<AdminController> logger)
        {
            this.logger = logger;
        }

   //     [HttpGet]
   //     public IEnumerable<ConnectedNode> Get()
   //     {
   //         List<ConnectedNode> nodes = new List<ConnectedNode>();
   //         nodes.Add(new ConnectedNode() { Name = "test1" });
			//return nodes;
   //     }

        [HttpGet]
        public string Get()
        {
            return "admin hello";
        }
    }
}
