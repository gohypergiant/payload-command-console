using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hypergiant.HIVE;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProxyStoreService.Data;

namespace ProxyStoreService.Controllers
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

        [HttpGet]
        public ActionResult<IEnumerable<ProxyCommand>> Get([FromQuery]Guid passid)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                List<ProxyCommand> c;

                if (passid.Equals(Guid.Empty))
                {
                    c = context.Commands.ToList();
                }
                else
                {
                    c = context.Commands.Where(c => c.GroundStationPassID.Equals(passid)).ToList();
                    if (c == null || c.Count() == 0)
                    {
                        return NotFound();
                    }
                }
                return c;
            }
        }

        [HttpPost]
        public async Task InsertCommand([FromBody]ProxyCommand command)
        {
            if (command == null)
            {
                this.BadRequest();
                return;
            }

            if (command.CommandID.Equals(Guid.Empty))
            {
                command.CommandID = Guid.NewGuid();
            }

            // make sure this will get synchronized
            command.IsSynchronized = false;

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                await context.Commands.AddAsync(command);
                context.SaveChanges();
            }

            this.Ok(JsonSerializer.Serialize(
                new
                {
                    CommandID = command.CommandID
                }));
        }

        [HttpPost("{commandID}/history")]
        public async Task<ActionResult> InsertCommandHistory([FromRoute]Guid commandID, [FromBody]ProxyCommandHistoryRecord history)
        {
            if (commandID == Guid.Empty)
            {
                return NotFound();
            }
            if (history == null)
            {
                return BadRequest("Invalid data");
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                // it's likely we only got a basic CommandHistory, so we need to populate any other bits
                if (history.CommandID == Guid.Empty)
                {
                    history.CommandID = commandID;
                }
                else if(history.CommandID != commandID)
                {
                    return BadRequest("CommandID in data does not match CommandID in route");
                }

                var cmd = context.Commands.Find(commandID);
                if (cmd != null)
                {
                    if (string.IsNullOrEmpty(history.DestinationID))
                    {
                        history.DestinationID = cmd.DestinationID;
                    }
                    if (history.GroundStationPassID == Guid.Empty)
                    {
                        history.GroundStationPassID = cmd.GroundStationPassID;
                    }
                }

                if (history.RecordID == Guid.Empty)
                {
                    history.RecordID = Guid.NewGuid();
                }

                // it's also possible we're receiving data we already know about, so filter it out
                var existing = context.CommandHistory.Where(h => h.CommandID == history.CommandID).ToList();
                if (!existing.Contains(history))
                {
                    await context.CommandHistory.AddAsync(history);
                }
                else
                {

                }
                context.SaveChanges();
            }

            return Accepted();
        }

        [HttpGet("{commandID}/history")]
        public ActionResult<ProxyCommandHistoryRecord[]> GetCommandHistory([FromRoute]Guid commandID)
        {
            if (commandID == Guid.Empty)
            {
                return NotFound();
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var c = context.CommandHistory.Where(c => c.CommandID.Equals(commandID)).ToList();
                if (c == null || c.Count() == 0)
                {
                    return NotFound();
                }

                return Ok(c);
            }
        }
    }
}