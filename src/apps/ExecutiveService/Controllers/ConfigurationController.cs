using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hypergiant.HIVE.HGSExecutive.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ConfigurationController : ControllerBase
    {
        private readonly ILogger<CommandsController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ConfigurationController(IServiceProvider serviceProvider, ILogger<CommandsController> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [HttpGet]
        public ExecutiveConfiguration Get()
        {
            var exec = _serviceProvider.GetService<ExecutiveProvider>().CurrentExecutive;

            if (exec is CfdpCommandExecutive)
            {
                var cfgc = exec as CfdpCommandExecutive;
                return new ExecutiveConfiguration
                {
                    ExecutiveType = "cfdp",
                    SatelliteAddress = cfgc.SatelliteAddress,
                    DestinationUplinkFolder = cfgc.UplinkRootFolder,
                    CfdpConfigFile = cfgc.CfdpConfigFile,
                    SatelliteCfdpEntityID = cfgc.SatelliteEntityID
                };
            }
            else if (exec is ScpCommandExecutive)
            {
                var cfg = exec as ScpCommandExecutive;
                return new ExecutiveConfiguration
                {
                    ExecutiveType = "scp",
                    SatelliteAddress = cfg.SatelliteAddress,
                    DestinationUplinkFolder = cfg.UplinkRootFolder,
                    UserName = cfg.UserName,
                    Password = "********"
                };
            }
            else
            {
                return new ExecutiveConfiguration
                {
                    ExecutiveType = exec == null ? "[not configured]" : exec.GetType().Name
                };
            }
        }

        [HttpPut]
        public ActionResult Put([FromBody]ExecutiveConfiguration config)
        {
            var exec = _serviceProvider.GetService<ExecutiveProvider>();
            exec.ConfigureExecutive(config);
            return Ok();
        }
    }
}
