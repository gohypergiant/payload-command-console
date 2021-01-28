using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProxyStoreService.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyStoreService.Services
{
    public abstract class DataSyncService : IHostedService
    {
        protected DataContext CommandContext { get; }

        public DataSyncService(IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            {
                CommandContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                CommandContext.Database.EnsureCreated();
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => SyncProc(cancellationToken));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async void SyncProc(CancellationToken cancellationToken)
        {
            while (true)
            {
                // TODO wait for a trigger or a timeout
                await Task.Delay(1000);

                if (cancellationToken.IsCancellationRequested) return;

                // get any updates that need to be pushed
                var items = CommandContext
                    .Commands
                    .Where(c => !c.IsSynchronized);

                foreach (var cmd in items)
                {
                    // push the update to the BE
                    if (await SendCommandToBackEnd(cmd))
                    {
                        // mark as synchronized and commit
                        cmd.IsSynchronized = true;
                        CommandContext.Update(cmd);
                        CommandContext.SaveChanges();
                    }
                    else
                    {
                        // TODO: what do we do here?
                        // TODO: log this
                    }
                }
            }
        }

        private async Task<bool> SendCommandToBackEnd(ProxyCommand command)
        {
            await Task.Delay(10);
            return true;
        }
    }
}
