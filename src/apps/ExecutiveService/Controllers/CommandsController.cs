using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hypergiant.HIVE.HGSExecutive.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CommandsController : ControllerBase
    {
        private readonly ILogger<CommandsController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public CommandsController(IServiceProvider serviceProvider, ILogger<CommandsController> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [HttpGet("{commandID}")]
        public ICommand Get()
        {
            return null;
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<CommandHistory>>> Post([FromBody]HGSCommandEnvelope command)
        {
            if (command.CommandID == Guid.Empty)
            {
                // TODO: should this be an error?
                command.CommandID = Guid.NewGuid();
            }

            var exec = _serviceProvider.GetService<ExecutiveProvider>().CurrentExecutive;
            if (exec == null)
            {
                throw new Exception("No executive configured");
            }
            var result = await exec.ExecuteOrDeliver(command);
            return Accepted(result);

        }
    }
}
