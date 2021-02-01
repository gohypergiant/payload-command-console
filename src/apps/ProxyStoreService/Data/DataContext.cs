using Microsoft.EntityFrameworkCore;

namespace ProxyStoreService.Data
{
    public class DataContext : DbContext
    {
        public DbSet<ProxyCommand> Commands { get; set; }
        public DbSet<ProxyCommandHistoryRecord> CommandHistory { get; set; }
        public DbSet<ProxyGroundStation> GroundStations { get; set; }
        public DbSet<ProxyGroundStationPass> Passes { get; set; }
        public DbSet<ProxyGroundStationPassHistoryRecord> PassHistory { get; set; }
        public DbSet<ProxyTelemetryMeta> TelemetryMeta { get; set; }
//        public DbSet<ProxyTelemetryRecord> TelemetryData { get; set; }

        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {

        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProxyGroundStation>().ToTable("GroundStations");
            modelBuilder.Entity<ProxyGroundStationPass>().ToTable("GroundStationPasses");
            modelBuilder.Entity<ProxyGroundStationPassHistoryRecord>().ToTable("GroundStationPassHistory");
            modelBuilder.Entity<ProxyCommandHistoryRecord>().ToTable("CommandHistory");
            modelBuilder.Entity<ProxyCommand>().ToTable("Commands");
            modelBuilder.Entity<ProxyTelemetryMeta>().ToTable("TelemetryMeta");
//            modelBuilder.Entity<ProxyTelemetryRecord>().ToTable("TelemetryData");
        }
    }
}
