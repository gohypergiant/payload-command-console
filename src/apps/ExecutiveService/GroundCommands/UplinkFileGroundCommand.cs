using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hypergiant.HIVE.HGSExecutive
{
    public class UplinkFileGroundCommand : HGSGroundCommandBase
    {
        private List<Tuple<string, string>> m_localToRemoteFileMap = new List<Tuple<string, string>>();
        private int RemoteEntityID { get; }

        public UplinkFileGroundCommand(Guid commandID, JsonElement e)
            : base(commandID)
        {
            if (!e.TryGetProperty("command", out JsonElement commandP))
            {
                throw new ArgumentException("invalid CFDP Uplink command data");
            }

            RemoteEntityID = e.GetInt32Property("remoteEntityID", -1);
            /*
            if (RemoteEntityID < 0)
            {
                throw new ArgumentException("No remote Entity ID provided");
            }
            */
            // do we have a file array?
            if (e.TryGetProperty("files", out JsonElement files))
            {
                var tempFolder = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp"));
                if (!tempFolder.Exists)
                {
                    tempFolder.Create();
                }

                foreach (var f in files.EnumerateArray())
                {
                    var n = f.GetStringProperty("remoteName", null);
                    var enc = f.GetStringProperty("encoding", null);
                    var c = f.GetStringProperty("contents", null);

                    var baseLocalPath = Path.Combine(tempFolder.FullName, Path.GetFileName(n));
                    var localPath = baseLocalPath;
                    var i = 1;
                    while (File.Exists(localPath))
                    {
                        localPath = $"{Path.GetFileNameWithoutExtension(baseLocalPath)}_{i++}{Path.GetExtension(baseLocalPath)}";
                    }

                    if (string.Compare(enc, "Base64", true) != 0)
                    {
                        throw new Exception($"Unsupported encoding: {enc}");
                    }

                    File.WriteAllBytes(localPath, Convert.FromBase64String(c));
                    m_localToRemoteFileMap.Add(new Tuple<string, string>(localPath, n));
                }
            }
            else
            {
                var local = e.GetStringProperty("localFile", null);
                if (string.IsNullOrEmpty(local))
                {
                    throw new ArgumentException("No local file name provided");
                }
                var remote = e.GetStringProperty("remoteFile", null);
                if (string.IsNullOrEmpty(remote))
                {
                    throw new ArgumentException("No remote file name provided");
                }

                m_localToRemoteFileMap.Add(new Tuple<string, string>(local, remote));
            }
        }

        public override async Task<IEnumerable<CommandHistory>> Execute(ISatelliteCommsService commsService)
        {
            if (commsService is CfdpCommandExecutive && RemoteEntityID < 0)
            {
                throw new Exception("CFDP Uplink Requires a Remote Entity ID");
            }

            var history = new List<CommandHistory>();
            history.Add(new CommandHistory
            {
                CommandID = this.CommandID,
                NewState = CommandState.Executing
            });


            foreach (var file in m_localToRemoteFileMap)
            {
                if (!File.Exists(file.Item1))
                {
                    history.Add(new CommandHistory
                    {
                        CommandID = this.CommandID,
                        Data = $"Local file '{file.Item1}' not found"
                    });
                }
                else
                {
                    if (await commsService.Uplink(file.Item1, file.Item2, RemoteEntityID))
                    {
                        history.Add(new CommandHistory
                        {
                            CommandID = this.CommandID,
                            Data = $"Transfer of {Path.GetFileName(file.Item1)} to '{file.Item2}' started"
                        });
                    }
                    else
                    {
                        history.Add(new CommandHistory
                        {
                            CommandID = this.CommandID,
                            NewState = CommandState.ExecutionFailed,
                            Data = $"Transfer of {Path.GetFileName(file.Item1)} to '{file.Item2}' +failed"
                        });
                    }
                }
            }

            return history;
        }
    }
}
