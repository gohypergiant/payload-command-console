using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Hypergiant.HIVE;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProxyStoreService.Data;

namespace ProxyStoreService.Controllers
{
    [ApiController]
    [Route("api/v1/history/passes")]
    public class PassHistoryController : ControllerBase
    {
        private readonly ILogger<CommandsController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public PassHistoryController(IServiceProvider serviceProvider, ILogger<CommandsController> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [HttpGet("{passID}")]
        public IEnumerable<GroundStationPassHistoryRecord> Get([FromRoute]Guid passID)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                    var history = context.PassHistory.Where(h => h.GroundStationPassID.Equals(passID)).ToArray();
                    return history;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        [HttpPost]
        public async Task Post([FromBody]ProxyGroundStationPassHistoryRecord history)
        {
            if (history == null)
            {
                this.BadRequest();
                return;
            }

            if (history.RecordID.Equals(Guid.Empty))
            {
                history.RecordID = Guid.NewGuid();
            }

            // make sure this will get synchronized
            history.IsSynchronized = false;

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                await context.PassHistory.AddAsync(history);
                context.SaveChanges();
            }

            this.Ok(JsonSerializer.Serialize(
                new
                {
                    history.RecordID
                }));
        }
    }
}
