using Hypergiant.HIVE.Support;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hypergiant.HIVE.HGSExecutive
{
    public abstract class HGSGroundCommandBase : IGroundCommand
    {
        public Guid CommandID { get; protected set; }

        /*
        public static T From<T>(JsonElement e)
            where T : IGroundCommand
        {
            return JsonSerializer.Deserialize<T>(e.GetRawText());
        }
        */

        public HGSGroundCommandBase(Guid commandID)
        {
            CommandID = commandID;
        }

        public abstract Task<IEnumerable<CommandHistory>> Execute(ISatelliteCommsService commsService);

        public static IGroundCommand From(HGSCommandEnvelope envelope, StorageProvider storageProvider)
        {
            Guid commandID = envelope.CommandID == Guid.Empty ? Guid.NewGuid() : envelope.CommandID;

            var commandType = envelope.ExecutionData.GetProperty("command").GetString().ToLower();
            switch (commandType.ToLower())
            {
                case "ping":
                    return new PingGroundCommand(commandID, envelope.ExecutionData);
                case "uplink":
                    return new UplinkFileGroundCommand(commandID, envelope.ExecutionData);
                case "get_cmd_hist":
                    return new DownlinkCommandHistoryGroundCommand(commandID, envelope.ExecutionData, storageProvider);
                case "get_telemetry":
                    return new DownlinkTelemetryGroundCommand(commandID, envelope.ExecutionData, storageProvider);
            }

            return null;
        }
    }
}
