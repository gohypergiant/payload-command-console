using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hypergiant.HIVE
{
    /*
    public class LocalFileCommandStore : CachingCommandStoreBase
    {
        private JsonSerializerOptions m_serializerOptions;

        public string StoreDirectory { get; }
        private DirectoryInfo CommandsDirectory { get; set; }
        private DirectoryInfo ResultsDirectory { get; set; }

        public LocalFileCommandStore(string storeDirectory, ILogService logService)
            : base(logService)
        {
            m_serializerOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            m_serializerOptions.Converters.Add(new JsonStringEnumConverter());

            if (string.IsNullOrEmpty(storeDirectory))
            {
                storeDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HIVE");
            }

            var di = new DirectoryInfo(storeDirectory);
            if (!di.Exists)
            {
                di.Create();
            }

            RehydrateCommands(di);
            RehydrateCommandResults(di);
        }

        protected override void BeforeAddCommand(ICommand command)
        {
            try
            {
                var json = JsonSerializer.Serialize(command, m_serializerOptions);
                var fileName = $"{(string.IsNullOrEmpty(command.Name) ? command.CommandID : command.Name)}.tcmd";
                var suffix = 1;
                while (CommandsDirectory.GetFiles(fileName).Length > 0)
                {
                    fileName = $"{(string.IsNullOrEmpty(command.Name) ? command.CommandID : command.Name)}_{suffix++}.tcmd";
                }
                using (var w = File.CreateText(Path.Combine(CommandsDirectory.FullName, fileName)))
                {
                    w.WriteLine(json);
                }
            }
            catch (Exception ex)
            {
                LogService?.Error(ex.Message);
            }
        }

        protected override void BeforeAddResult(ICommandResult result)
        {
            try
            {
                var json = JsonSerializer.Serialize(result);
                // TODO: maybe try to get the file name to coordinate with the command?
                var fileName = $"{result.CommandID}.tres";
                var path = Path.Combine(ResultsDirectory.FullName, fileName);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                using (var w = File.CreateText(path))
                {
                    w.WriteLine(json);
                }
            }
            catch (Exception ex)
            {
                LogService?.Error(ex.Message);
            }
        }

        protected override void BeforeUpdateResult(ICommandResult result)
        {
            // just overwrite the whole file out of laziness
            BeforeAddResult(result);
        }

        protected override void AfterRemoveCommand(string commandID)
        {
            // TODO
        }

        protected override void AfterRemoveResult(string commandID)
        {
            var fileName = $"{commandID}.tres";
            var path = Path.Combine(ResultsDirectory.FullName, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private void RehydrateCommands(DirectoryInfo root)
        {
            CommandsDirectory = root.CreateSubdirectory("Commands");

            foreach (var file in CommandsDirectory.GetFiles())
            {
                try
                {
                    var json = File.ReadAllText(file.FullName);
                    var command = JsonSerializer.Deserialize<Command>(json, m_serializerOptions);
                    AddCommandToCache(command);
                }
                catch (Exception ex)
                {
                    LogService?.Error(ex.Message);
                }
            }
        }

        private void RehydrateCommandResults(DirectoryInfo root)
        {
            ResultsDirectory = root.CreateSubdirectory("Results");

            foreach (var file in ResultsDirectory.GetFiles())
            {
                try
                {
                    var json = File.ReadAllText(file.FullName);
                    var command = JsonSerializer.Deserialize<CommandResult>(json, m_serializerOptions);
                    AddResultToCache(command);
                }
                catch (Exception ex)
                {
                    LogService?.Error(ex.Message);
                }
            }
        }
    }
    */
}
