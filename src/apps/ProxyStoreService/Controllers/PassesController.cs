using System;
using System.Collections.Generic;
using System.Linq;
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
    [Route("api/v1/[controller]")]
    public class PassesController : ControllerBase
    {
        private readonly ILogger<CommandsController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public PassesController(IServiceProvider serviceProvider, ILogger<CommandsController> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<ProxyGroundStationPass>> GetAllPasses()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                return await context.Passes.ToListAsync();
            }
        }

        [HttpGet("{passID}")]
        public ActionResult<ProxyGroundStationPass> GetPass([FromRoute]Guid passID)
        {
            if(passID == null)
            {
                return NotFound();
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var pass = context.Passes.Find(passID);
                if (pass == null)
                {
                    return NotFound();
                }
                return pass;
            }
        }

        [HttpPut("{passID}")]
        public async Task<ActionResult> Put([FromRoute]Guid passID, [FromBody]GroundStationPass pass)
        {
            if (pass == null)
            {
                return BadRequest();
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var existing = await context.Passes.FindAsync(passID);

                if (existing == null)
                {
                    return NotFound();
                }

                // these are the only updateable fields
                if (pass.ActualAos.HasValue)
                {
                    existing.ActualAos = pass.ActualAos.Value;
                }
                if (pass.ActualLos.HasValue)
                {
                    existing.ActualLos = pass.ActualLos.Value;
                }
                await context.SaveChangesAsync();

                return Accepted();
            }
        }

        [HttpPost]
        public ActionResult Post([FromBody]GroundStationPass pass)
        {
            if (pass == null)
            {
                return BadRequest();
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
            }

            return Accepted();
        }
    }
}
