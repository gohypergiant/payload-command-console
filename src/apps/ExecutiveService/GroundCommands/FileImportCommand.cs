using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hypergiant.HIVE.HGSExecutive
{
    public abstract class FileImportCommand : HGSGroundCommandBase
    {
        protected StorageProvider StorageProvider { get; }

        public FileImportCommand(Guid commandID, StorageProvider storageProvider)
            : base(commandID)
        {
            StorageProvider = storageProvider;
        }

        protected async Task DiscoverAndImport(DirectoryInfo di, List<CommandHistory> historyList)
        {
            var opts = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            opts.Converters.Add(new JsonStringEnumConverter());

            // look for all known data in the target folder, import it, and clean up
            if (!di.Exists) return;

            // first, unzip and zipped files
            var list = di.GetFiles("*zip");
            for (int i = 0; i < list.Length; i++)
            {
                UnzipFile(list[i].FullName, di.FullName);
                list[i].Delete();
            }

            var files = di.GetFiles();
            foreach (var f in files)
            {
                var count = 0;

                var doc = JsonDocument.Parse(File.ReadAllText(f.FullName));

                // look for command history
                if (doc.RootElement.TryGetProperty("commandHistory", out JsonElement h))
                {
                    foreach (var r in h.EnumerateArray())
                    {
                        var ch = JsonSerializer.Deserialize<CommandHistory>(r.ToString(), opts);

                        if (ch != null && ch.CommandID != Guid.Empty)
                        {
                            // pass this data to the store
                            if (await StorageProvider.AddCommandHistory(ch))
                            {
                                count++;
                            }
                        }
                    }

                    historyList?.Add(new CommandHistory(this.CommandID)
                    {
                        Data = $"Received {count} records for Command {Path.GetFileNameWithoutExtension(f.Name)}"
                    });

                    f.Delete();
                }
                // look for telemetry data
                else if (doc.RootElement.TryGetProperty("path", out JsonElement p) && doc.RootElement.TryGetProperty("values", out JsonElement v))
                {
                    var path = p.GetString();

                    if (await StorageProvider.AddTelemetry(doc.RootElement))
                    {
                        count = v.GetArrayLength();
                        historyList?.Add(new CommandHistory(this.CommandID)
                        {
                            Data = $"Received {count} telemetry items for {path}"
                        });
                    }
                    f.Delete();
                }
                else if(f.Name == "telemetry_meta.json")
                {
                    await StorageProvider.AddTelemetryMeta(doc.RootElement);

                    historyList?.Add(new CommandHistory(this.CommandID)
                    {
                        Data = $"Received telemetry structure"
                    });
                }
                else
                {
                    historyList?.Add(new CommandHistory(this.CommandID)
                    {
                        Data = $"File '{f.Name}' contains unknown content. It is still at '{f.FullName}'"
                    });
                }
            }
        }

        private void UnzipFile(string archivePath, string destination)
        {
            string f;
            string actualDest;

            using (var zip = ZipFile.Open(archivePath, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    var suffix = 0;
                    do
                    {
                        f = $"{Path.GetFileNameWithoutExtension(entry.Name)}{(suffix++ > 0 ? $"_{suffix}" : string.Empty)}{Path.GetExtension(entry.Name)}";
                        actualDest = Path.Combine(destination, f);
                    } while (File.Exists(actualDest));

                    entry.ExtractToFile(actualDest);
                }

                zip.Dispose();
            }
        }

    }
}
