using Microsoft.VisualBasic.CompilerServices;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hypergiant.HIVE.HGSExecutive
{
    public class ScpCommandExecutive : CommandExecutiveBase
    {
        private ScpClient Client { get; }

        public string UserName { get; }

        public ScpCommandExecutive(
            StorageProvider storageProvider,
            string satelliteAddress,
            string uplinkRootFolder,
            string username,
            string password)
            : base(storageProvider, satelliteAddress, uplinkRootFolder)
        {
            UserName = username;
            Client = new ScpClient(SatelliteAddress, UserName, password);
            Client.ErrorOccurred += Client_ErrorOccurred;
        }

        private void Client_ErrorOccurred(object sender, Renci.SshNet.Common.ExceptionEventArgs e)
        {
        }

        public override IEnumerable<CommandHistory> DeliverCommandToSatellite(ICommandEnvelope command)
        {
            var s = new List<CommandHistory>();

            try
            {
                if (!Client.IsConnected)
                {
                    Client.Connect();
                }

                var commandObject = new
                {
                    command.CommandID,
                    command.ExecutionData
                };

                var opts = new JsonSerializerOptions
                {
                    IgnoreNullValues = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                opts.Converters.Add(new JsonStringEnumConverter());

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(commandObject, opts))))
                {
                    // create the destination name
                    var p = Path.Combine(UplinkRootFolder, command.Name).Replace('\\', '/');
                    // send the command stream
                    Client.Upload(stream, Path.Combine(UplinkRootFolder, p));
                }
                s.Add(new CommandHistory(command.CommandID)
                {
                    Data = "Command Uplinked."
                });
            }
            catch (Exception ex)
            {
                s.Add(new CommandHistory(command.CommandID)
                {
                    Data = $"Uplink failed: {ex.Message}"
                });
            }
            return s;
        }

        public override Task<bool> Uplink(string localFile, string remoteFile, int remoteEntityID)
        {
            if (!Client.IsConnected)
            {
                Client.Connect();
            }

            var fi = new FileInfo(localFile);
            if (!fi.Exists)
            {
                throw new Exception($"Source file '{localFile}' not found");
            }

            Client.Upload(fi, remoteFile);

            return Task.FromResult(true);
        }

        public override Task<bool> Downlink(string remoteFile, string localFile, int remoteEntityID = 0)
        {
            if (!Client.IsConnected)
            {
                Client.Connect();
            }

            Client.Download(remoteFile, new DirectoryInfo(localFile));
            return Task.FromResult(true);
        }
    }
}
