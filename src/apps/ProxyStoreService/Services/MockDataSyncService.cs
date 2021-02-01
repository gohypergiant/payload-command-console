using ProxyStoreService.Data;
using System;
using System.Linq;

namespace ProxyStoreService.Services
{
    public class MockDataSyncService : DataSyncService
    {
        public MockDataSyncService(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            // create some ground stations
            CreateMockGroundStations();
            CreateMockPasses();
        }

        private void CreateMockGroundStations()
        {
            if (CommandContext.GroundStations.Count() == 0)
            {
                CommandContext.GroundStations.AddRange(
                    new ProxyGroundStation
                    {
                        GroundStationID = Guid.NewGuid(),
                        Name = "Wallops",
                        IsSynchronized = true,
                        Latitude = 37.923529f,
                        Longitude = -75.476132f
                    },
                    new ProxyGroundStation
                    {
                        GroundStationID = Guid.NewGuid(),
                        Name = "Hawaii",
                        IsSynchronized = true,
                        Latitude = 19.014302f,
                        Longitude = -155.663375f
                    });
                CommandContext.SaveChanges();
            }
        }

        private void CreateMockPasses()
        {
            // always create a few future passes
            var now = DateTime.Now.ToUniversalTime();
            var gsIDs = CommandContext.GroundStations.Select(c => c.GroundStationID).ToArray();
            var rnd = new Random();

            CommandContext.Passes.AddRange(
                new ProxyGroundStationPass
                {
                    GroundStationPassID = Guid.NewGuid(),
                    GroundStationID = gsIDs[0],
                    IsSynchronized = true,
                    ExpectedElevation = rnd.Next(150, 900) / 10m,
                    ScheduledAos = now.AddMinutes(rnd.Next(2, 6)),
                    ScheduledLos = now.AddMinutes(rnd.Next(10, 15)).AddSeconds(rnd.Next(0, 60))
                },
                new ProxyGroundStationPass
                {
                    GroundStationPassID = Guid.NewGuid(),
                    GroundStationID = gsIDs[1],
                    IsSynchronized = true,
                    ExpectedElevation = rnd.Next(150, 900) / 10m,
                    ScheduledAos = now.AddMinutes(rnd.Next(90, 95)),
                    ScheduledLos = now.AddMinutes(rnd.Next(96, 100)).AddSeconds(rnd.Next(0, 60))
                },
                new ProxyGroundStationPass
                {
                    GroundStationPassID = Guid.NewGuid(),
                    GroundStationID = gsIDs[0],
                    IsSynchronized = true,
                    ExpectedElevation = rnd.Next(150, 900) / 10m,
                    ScheduledAos = now.AddMinutes(rnd.Next(183, 188)),
                    ScheduledLos = now.AddMinutes(rnd.Next(189, 197)).AddSeconds(rnd.Next(0, 60))
                });
            CommandContext.SaveChanges();
        }
    }
}
