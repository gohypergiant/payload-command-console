using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hypergiant.HIVE.HGSExecutive
{
    public class CfdpDownlinkGroundCommand : HGSGroundCommandBase
    {
        private string LocalFile { get; }
        private string RemoteFile { get; }
        private int RemoteEntityID { get; }

        public CfdpDownlinkGroundCommand(Guid commandID, JsonElement e)
            : base(commandID)
        {
            /*
            {
                "command":"downlink",
                "localFile":"c:\\temp\\hive\\bricks.jpg",
                "remoteFile":"c:\\temp\\hive\\bricks_sent.jpg",
                "remoteEntityID":1
            }
            */
            if (!e.TryGetProperty("command", out JsonElement commandP))
            {
                throw new ArgumentException("invalid CFDP Uplink command data");
            }

            LocalFile = e.GetStringProperty("localFile", null);
            if (string.IsNullOrEmpty(LocalFile))
            {
                throw new ArgumentException("No local file name provided");
            }
            RemoteFile = e.GetStringProperty("remoteFile", null);
            if (string.IsNullOrEmpty(RemoteFile))
            {
                throw new ArgumentException("No remote file name provided");
            }
            RemoteEntityID = e.GetInt32Property("remoteEntityID", -1);
            if (RemoteEntityID < 0)
            {
                throw new ArgumentException("No remote Entity ID provided");
            }
        }

        public override async Task<IEnumerable<CommandHistory>> Execute(ISatelliteCommsService commsService)
        {

            throw new NotImplementedException();
        }
    }
}
