using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Hypergiant.HIVE;
using PayloadCommandConsole.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PayloadCommandConsole
{
    public interface ICommandExecutionService
    {
        event CommandHistoryChangedHandler CommandHistoryChanged;

        int ExecutivePort { get; set; }
        Task<bool> DeliverCommand(ObservableCommand command);
        Dictionary<Guid, CommandHistory> History { get; }
        Guid CurrentCommandID { get; set; }
        ICommandEnvelope Parse(string command);

        Task<ExecutiveConfiguration> GetExecutiveConfiguration();
        Task<bool> UpdateExecutiveConfiguration(ExecutiveConfiguration configuration);
    }

    public delegate void CommandHistoryChangedHandler(CommandHistory history);

    public class HGSCommandExecutionService : ICommandExecutionService
    {
        private AutoResetEvent m_commandEvent = new AutoResetEvent(false);
        private HGSExecutiveMicroservice m_service;
        private JsonSerializerOptions m_options;
        private INotificationManager m_notificationManager;

        // Map of CommandID->StateData
        public Dictionary<Guid, CommandHistory> History { get; } = new Dictionary<Guid, CommandHistory>();
        public Guid CurrentCommandID { get; set; }
        public event CommandHistoryChangedHandler CommandHistoryChanged;

        public HGSCommandExecutionService(INotificationManager notificationManager)
        {
            m_notificationManager = notificationManager;

            m_options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true
            };

            // TODO: load user settings
            m_service = new HGSExecutiveMicroservice(5002);

            m_service.Start();

            Task.Run(() => CommandSyncProc());
        }

        public int ExecutivePort
        {
            get => m_service.ServicePort;
            set => m_service.ServicePort = value;
        }

        public ICommandEnvelope Parse(string command)
        {
            return HGSCommandEnvelope.Parse(command);
        }

        public async Task<bool> DeliverCommand(ObservableCommand command)
        {
            if (History.ContainsKey(command.CommandID))
            {
                throw new Exception("Command has already been run");
            }

            var envelope = JsonSerializer.Deserialize<HGSCommandEnvelope>(command.Payload, m_options);
            if (await m_service.DeliverCommand(envelope))
            {
                // TODO: update state
                //                History.Add(command.CommandID, new CommandStateData(command.CommandID));
                m_commandEvent.Set();

                return true;
            }

            return false;
        }

        private async Task CommandSyncProc()
        {
            while (true)
            {
                // check every 2 second (or faster if we know arresult might be waiting)
                m_commandEvent.WaitOne(2000);

                if (!CurrentCommandID.Equals(Guid.Empty))
                {
                    try
                    {
                        //                        await RefreshCommandHistory(CurrentCommandID);
                    }
                    catch (Exception ex)
                    {
                        _ = Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            m_notificationManager?.Show(new Notification("Error", "Execution Service is offline"));
                        });

                        // TODO: log
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task<ExecutiveConfiguration> GetExecutiveConfiguration()
        {
            // first try the service
            var cfg = await m_service.GetExecutiveConfiguration();
            if (cfg == null)
            {
                // TODO: inform the user it was null (error?)
                return GetDefaultExecutiveConfiguration();
            }
            else if (cfg.ExecutiveType.StartsWith('['))
            {
                return GetDefaultExecutiveConfiguration();
            }
            else
            {
                return cfg;
            }
        }

        private ExecutiveConfiguration GetDefaultExecutiveConfiguration()
        {
            return new ExecutiveConfiguration
            {
                ExecutiveType = "scp",
                DestinationUplinkFolder = "/home/pi/uplink",
                UserName = "mando",
                Password = "mando",
                SatelliteAddress = "10.3.1.10",
                CfdpConfigFile = "cfdp-config.txt",
                SatelliteCfdpEntityID = 1
            };
        }

        public async Task<bool> UpdateExecutiveConfiguration(ExecutiveConfiguration configuration)
        {
            return await m_service.UpdateExecutiveConfiguration(configuration);
        }
    }
}
