using DynamicData.Binding;
using Hypergiant.HIVE;
using PayloadCommandConsole.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PayloadCommandConsole
{
    public class CommandStore : IDisposable
    {
        // dictionary of passID->command
        public Dictionary<Guid, ObservableCollectionExtended<ObservableCommand>> Commands { get; }
        // dictionary of command ID->command history
        public Dictionary<Guid, ObservableCollectionExtended<CommandHistory>> CommandHistory { get; }
        public ObservableCollectionExtended<ObservablePass> Passes { get; }

        private HttpClient m_client;
        private JsonSerializerOptions m_options;
        private ProxyStoreMicroservice m_service;

        public CommandStore(bool mockData)
        {
            m_service = new ProxyStoreMicroservice();
            m_service.Start();

            Commands = new Dictionary<Guid, ObservableCollectionExtended<ObservableCommand>>();
            CommandHistory = new Dictionary<Guid, ObservableCollectionExtended<CommandHistory>>();
            Passes = new ObservableCollectionExtended<ObservablePass>();

            m_client = new HttpClient();

            m_options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true
            };
            m_options.Converters.Add(new JsonStringEnumConverter());

            if (mockData)
            {
                GenerateMockPasses();
            }
            else
            {
                Task.Run(() => RefreshPasses());
            }
        }

        public void Dispose()
        {
            m_service.Stop();
        }

        public async Task<bool> ImportCommand(ICommandEnvelope command, Guid passID)
        {
            try
            {
                var cmd = new Command
                {
                    CommandID = command.CommandID,
                    Name = command.Name,
                    CreatedBy = Guid.Empty, // TODO
                    CreatedOn = DateTime.Now.ToUniversalTime(),
                    DestinationID = "[unknown]", // TODO
                    CurrentState = CommandState.AwaitingApproval,
                    SourceID = "[HIVE console]", // TODO
                    GroundStationPassID = passID,
                    RawPayload = command.ToRawPayload()
                };

                if (await m_service.InsertCommand(cmd))
                {
                    var history = new CommandHistory
                    {
                        CommandID = cmd.CommandID,
                        NewState = cmd.CurrentState,
                        Timestamp = DateTime.UtcNow,
                        Data = "Imported at Proxy"
                    };

                    await m_service.InsertCommandHistory(history);

                    // trigger a Command refresh
                    _ = RefreshCommands(passID);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                // TODO: log
                return false;
            }
        }

        public void VerifyPass(Guid passID)
        {
            if (!Commands.ContainsKey(passID))
            {
                Commands.Add(passID, new ObservableCollectionExtended<ObservableCommand>());
            }
        }

        public async Task RefreshCommands(Guid passID)
        {
            var commands = await m_service.GetCommandsForPass(passID);

            VerifyPass(passID);

            var newIDSet = commands.Select(c => c.CommandID);
            var oldIDSet = Commands[passID].Select(c => c.CommandID);

            var removeIDs = oldIDSet.Except(newIDSet);
            var checkIDs = oldIDSet.Intersect(newIDSet);
            var addIDs = newIDSet.Except(oldIDSet);

            // look for removals
            foreach (var id in removeIDs)
            {
                Commands[passID].Remove(Commands[passID].First(c => c.CommandID.Equals(id)));
            }

            foreach (var existing in Commands[passID].Where(c => checkIDs.Contains(c.CommandID)))
            {
                var check = commands.First(c => c.CommandID == existing.CommandID);
                try
                {
                    existing.CopyPropertiesFrom(check);
                }
                catch (Exception ex)
                {
                    // TODO: log
                    Debug.Write(ex.Message);
                }
            }

            // add the new items
            foreach (var cmd in commands.Where(c => addIDs.Contains(c.CommandID)))
            {
                Commands[passID].Add(new ObservableCommand(cmd));
            }
        }

        private async Task RefreshPasses()
        {
            try
            {
                var groundStations = await m_service.GetGroundStations();
                var passes = await m_service.GetPasses();

                var result = (from p in passes
                              join g in groundStations on p.GroundStationID equals g.GroundStationID
                              select new ObservablePass(p, g)).ToList();

                foreach (var pass in result)
                {
                    var existing = Passes.FirstOrDefault(p => p.PassID.Equals(pass.PassID));
                    if (existing == null)
                    {
                        var history = await m_service.GetPassHistory(pass.PassID);

                        if (history != null)
                        {
                            foreach (var h in history)
                            {
                                switch (h.Action)
                                {
                                    case PassAction.GoForCommand:
                                        pass.ActualGoCmd = h.TimestampUtc;
                                        break;
                                    case PassAction.Note:
                                        pass.Notes.Add(h.Summary);
                                        break;
                                }

                            }
                        }
                        Passes.Add(pass);
                    }
                    else
                    {
                        // TODO:
                        // look for changes
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task RecordPassHistoryRecord(Guid passID, DateTime timestampUtc, PassAction action, string summary = null, string details = null)
        {
            try
            {
                switch (action)
                {
                    case PassAction.AOS:
                        await m_service.UpdatePassAos(passID, timestampUtc);

                        // TODO: check for success

                        break;
                    case PassAction.LOS:
                        await m_service.UpdatePassLos(passID, timestampUtc);

                        // TODO: check for success
                        break;
                }

                var h = new GroundStationPassHistoryRecord
                {
                    RecordID = Guid.NewGuid(),
                    GroundStationPassID = passID,
                    TimestampUtc = timestampUtc,
                    Summary = summary,
                    Detail = details,
                    DetailType = HistoryDetailType.Text,
                    UserID = Guid.Empty, // TODO:
                    Action = action
                };

                await m_service.InsertPassHistory(h);
                // TODO: check for success

                // the result on success is the record ID
                // store it locally in-proc?  we have no local history viewer, so no need right now
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void GenerateMockPasses()
        {
            Passes.AddRange(
                new ObservablePass[]
                {
                    new ObservablePass(Guid.NewGuid())
                    {
                         StationName = "Wallops",
                         ExpectedElevation= 82.5m,
                         ScheduledAos = DateTime.Now.AddMinutes(3).ToUniversalTime(),
                         ScheduledLos = DateTime.Now.AddMinutes(11).AddSeconds(13).ToUniversalTime()
                    },
                    new ObservablePass(Guid.NewGuid())
                    {
                         StationName = "Hawaii",
                         ExpectedElevation= 18.3m,
                         ScheduledAos = DateTime.Now.AddMinutes(84).ToUniversalTime(),
                         ScheduledLos = DateTime.Now.AddMinutes(96).AddSeconds(7).ToUniversalTime()
                    },
                    new ObservablePass(Guid.NewGuid())
                    {
                         StationName = "Wallops",
                         ExpectedElevation= 26.2m,
                         ScheduledAos = DateTime.Now.AddMinutes(193).ToUniversalTime(),
                         ScheduledLos = DateTime.Now.AddMinutes(194).AddSeconds(45).ToUniversalTime()
                    },
                });
        }

        public async Task RefreshCommandHistory(Guid commandID)
        {
            var history = await m_service.GetCommandHistory(commandID);

            // for now history is purely additive, so just look for a length change
            if (!CommandHistory.ContainsKey(commandID))
            {
                CommandHistory.Add(commandID, new ObservableCollectionExtended<CommandHistory>(history));
            }
            else
            {
                if (CommandHistory[commandID].Count() < history.Length)
                {
                    foreach (var h in history)
                    {
                        var existing = CommandHistory[commandID].FirstOrDefault(r => r.Timestamp == h.Timestamp);
                        if (existing != null) continue;
                        CommandHistory[commandID].Add(h);
                    }
                }
            }
        }

        /*
        private async Task RefreshCommandHistory(Guid commandID)
        {
            var result = await m_service.GetCommandHistory(commandID);
            if (result == null)
            {
                return;
            }

            var changed = false;

            if (!History.ContainsKey(result.CommandID))
            {
                History.Add(result.CommandID, result);

                // TODO: ensure these are forwarded to the store

                changed = true;
            }
            else
            {
                // update the existing result
                var existing = History[result.CommandID];
                if (existing.ExecutionStart != result.ExecutionStart)
                {
                    existing.ExecutionStart = result.ExecutionStart;
                    changed = true;
                }
                if (existing.ExecutionEnd != result.ExecutionEnd)
                {
                    existing.ExecutionEnd = result.ExecutionEnd;
                    changed = true;
                }

                // right now the list is only ever added to, so we don't need to look for changes
                if (result.Info.Count != existing.Info.Count)
                {
                    for (int i = existing.Info.Count; i < result.Info.Count; i++)
                    {
                        existing.Info.Add(result.Info[i]);
                    }
                    changed = true;
                }

                // TODO: ensure these are forwarded to the store
            }

            if (changed)
            {
                CommandHistoryChanged?.Invoke(result);
            }
        }
        */
    }
}
