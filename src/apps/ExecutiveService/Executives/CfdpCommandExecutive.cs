using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hypergiant.HIVE.HGSExecutive
{
    public interface ISatelliteCommsService
    {
        Task<bool> Uplink(string localFile, string remoteFile, int remoteEntityID = 0);
        Task<bool> Downlink(string remoteFile, string localFile, int remoteEntityID = 0);
    }

    public class CfdpCommandExecutive : CommandExecutiveBase, ISatelliteCommsService
    {
        public string CfdpConfigFile { get; }
        public int SatelliteEntityID { get; }
        private CfdpClient CfdpClient { get; }

        private JsonSerializerOptions m_options;

        public CfdpCommandExecutive(
            StorageProvider storageProvider,
            string satelliteAddress,
            string uplinkRootFolder,
            string cfdpConfigFile,
            int satelliteEntityID)
            : base(storageProvider, satelliteAddress, uplinkRootFolder)
        {
            SatelliteEntityID = satelliteEntityID;

            m_options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true
            };
            m_options.Converters.Add(new JsonStringEnumConverter());

            var fi = new FileInfo(CfdpConfigFile);

            if(!fi.Exists)
            {
                throw new Exception($"CFDP Config File '{fi.Name}' not found");
            }

            CfdpConfigFile = fi.FullName;
            CfdpClient = new CfdpClient(fi.FullName);
        }

        public override IEnumerable<CommandHistory> DeliverCommandToSatellite(ICommandEnvelope command)
        {
            var history = new List<CommandHistory>();

            try
            {
                var commandObject = new
                {
                    command.CommandID,
                    command.ExecutionData
                };

                // CFDP requires just filenames, so store to a local temp file
                var tempSourceName = Path.Combine(Path.GetTempPath(), command.CommandID.ToString());
                if (File.Exists(tempSourceName))
                {
                    // since IDs are unique, this probably shouldn't happen, but protect anyway
                    File.Delete(tempSourceName);
                }

                using (var s = File.CreateText(tempSourceName))
                {
                    s.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(commandObject, m_options)));
                }

                var destinationName = Path.Combine(UplinkRootFolder, command.Name).Replace('\\', '/');

                CfdpClient.Put(tempSourceName, destinationName, SatelliteEntityID, CfdpClass.Class2);

                // TODO: queue deletion of temp file for after delivery (need to add support to CFDP to know when that happens)

                history.Add(new CommandHistory(command.CommandID)
                {
                    Data = "Command Sent."
                });
            }
            catch (Exception ex)
            {
                history.Add(new CommandHistory(command.CommandID)
                {
                    Data = $"Uplink failed: {ex.Message}"
                });
            }
            return history;
        }

        public override Task<bool> Downlink(string remoteFile, string localFile, int remoteEntityID = 0)
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> Uplink(string localFile, string remoteFile, int remoteEntityID)
        {
            throw new NotImplementedException();
        }
    }
}
