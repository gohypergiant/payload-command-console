using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Hypergiant.HIVE;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProxyStoreService.Data;

namespace ProxyStoreService.Controllers
{
    public class TelemetryCollection
    {
        public string Path { get; set; }
        public List<JsonElement> Values { get; set; }
    }

    [ApiController]
    [Route("api/v1/[controller]")]
    public class TelemetryController : ControllerBase
    {
        private readonly ILogger<CommandsController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public TelemetryController(IServiceProvider serviceProvider, ILogger<CommandsController> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [HttpPost("data")]
        public async Task<ActionResult> AddTelemetryData([FromBody]TelemetryCollection collection)
        {
            /*
            var path = p.GetString();

            foreach (var r in v.EnumerateArray())
            {
                if (await StorageProvider.AddTelemetry(path, r))
                {
                    count++;
                }
            }
            */

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                // see if we already know about this data (or do we care - we could let the back end figure that out)
//                var data = new ProxyTelemetryRecord();
//                await context.TelemetryData.AddAsync(data);
                context.SaveChanges();
            }

            return Accepted();
        }

        [HttpPost("meta")]
        public async Task<ActionResult> AddTelemetryMeta([FromBody]JsonElement meta)
        {
            var opts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IgnoreNullValues = true
            };
            opts.Converters.Add(new JsonStringEnumConverter());
            
            var sys = JsonSerializer.Deserialize<TelemetrySystem>(meta.ToString(), opts);

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                // see if we already know about this data (or do we care - we could let the back end figure that out)
                var data = new ProxyTelemetryMeta()
                {
                    SatelliteID = "[not set]",
                    MetaData = meta.ToString()
                };

                await context.TelemetryMeta.AddAsync(data);
                context.SaveChanges();
            }

            return Accepted();
        }
    }
}