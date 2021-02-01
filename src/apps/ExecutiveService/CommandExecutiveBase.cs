using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Hypergiant.HIVE.HGSExecutive
{
    public abstract class CommandExecutiveBase : IHostedService, ISatelliteCommsService, ICommandExecutive
    {
        public string SatelliteAddress { get; }
        public string UplinkRootFolder { get; }
        public StorageProvider StorageProvider { get; }

        public abstract IEnumerable<CommandHistory> DeliverCommandToSatellite(ICommandEnvelope command);
        public abstract Task<bool> Uplink(string localFile, string remoteFile, int remoteEntityID = 0);
        public abstract Task<bool> Downlink(string remoteFile, string localFile, int remoteEntityID = 0);

        public CommandExecutiveBase(StorageProvider storageProvider, string satelliteAddress, string uplinkRootFolder)
        {
            StorageProvider = storageProvider;
            SatelliteAddress = satelliteAddress;
            UplinkRootFolder = uplinkRootFolder;
        }

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public virtual Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public virtual async Task<IEnumerable<CommandHistory>> ExecuteOrDeliver(HGSCommandEnvelope command)
        {
            IEnumerable<CommandHistory> results;

            // is the command a local?  if so, execute
            if (!string.IsNullOrEmpty(command.ExecuteLocation)
                && string.Compare(command.ExecuteLocation, "ground", true) == 0)
            {
                results = await ExecuteLocal(command);
            }
            else
            {
                results = DeliverCommandToSatellite(command);
            }

            await StorageProvider.AddCommandHistory(results);

            return results;
        }

        protected virtual void BeforeExecuteLocal(IGroundCommand command) { }

        private async Task<IEnumerable<CommandHistory>> ExecuteLocal(HGSCommandEnvelope command)
        {
            var cmd = HGSGroundCommandBase.From(command, StorageProvider);
            IEnumerable<CommandHistory> result;
            try
            {
                result = await cmd.Execute(this);
            }
            catch (Exception ex)
            {
                result = new CommandHistory[]
                {
                    new CommandHistory
                    {
                         CommandID = command.CommandID,
                         NewState = CommandState.ExecutionFailed,
                         Data = $"Failed: {ex.Message}"
                    }
                };
            }
            return result;
        }
    }
}
