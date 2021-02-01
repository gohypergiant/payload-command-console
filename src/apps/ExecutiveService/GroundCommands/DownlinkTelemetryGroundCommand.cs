using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hypergiant.HIVE.HGSExecutive
{
    public class DownlinkTelemetryGroundCommand : FileImportCommand
    {
        public DownlinkTelemetryGroundCommand(Guid commandID, JsonElement _, StorageProvider storageProvider)
            : base(commandID, storageProvider)
        {
        }

        public override async Task<IEnumerable<CommandHistory>> Execute(ISatelliteCommsService commsService)
        {

            var state = new List<CommandHistory>();
            state.Add(new CommandHistory(this.CommandID)
            {
                NewState = CommandState.Executing
            });

            if (commsService is ScpCommandExecutive)
            {
                var tempFolder = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp"));
                if (!tempFolder.Exists)
                {
                    tempFolder.Create();
                }

                // create a folder for these files
                var di = tempFolder.CreateSubdirectory(DateTime.Now.ToString("yyyyMMddHHmmss"));

                try
                {
                    // we can get with a wildcard
                    state.Add(new CommandHistory(this.CommandID)
                    {
                        Data = $"Begin telemetry file download..."
                    });
                    // TODO: this needs to come from config
                    await commsService.Downlink("/home/pi/downlink/telemetry", di.FullName);
                    var files = di.GetFiles();
                    state.Add(new CommandHistory(this.CommandID)
                    {
                        Data = $"Downloaded {files.Length} Telemetry files"
                    });

                    await DiscoverAndImport(di, state);
                }
                catch (Exception ex)
                {
                    state.Add(new CommandHistory(this.CommandID)
                    {
                        Data = $"Failed: {ex.Message}"
                    });
                }

                // if we successfully processed everything, the directory will be empty
                di.Refresh();
                if (di.GetFiles().Length == 0)
                {
                    di.Delete();
                }

                state.Add(new CommandHistory(this.CommandID)
                {
                    NewState = CommandState.ExecutionComplete
                });
            }
            else
            {

                throw new System.NotImplementedException();
            }

            return state;
        }
    }
}
