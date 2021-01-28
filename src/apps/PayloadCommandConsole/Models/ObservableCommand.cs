using DynamicData.Binding;
using Hypergiant.HIVE;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PayloadCommandConsole.Models
{
    public class ObservablePass : AbstractNotifyPropertyChanged
    {
        public Guid PassID { get; set; }
        public string StationName { get; set; }
        public DateTime ScheduledAos { get; set; }
        public DateTime? ScheduledLos { get; set; }
        public decimal? ExpectedElevation { get; set; }
        public DateTime? ActualAos { get; set; }
        public DateTime? ActualGoCmd { get; set; }
        public DateTime? ActualLos { get; set; }
        public ObservableCollection<string> Notes { get; set; } = new ObservableCollection<string>();


        public ObservablePass(Guid passID)
        {
            PassID = passID;
        }

        public ObservablePass(GroundStationPass pass, GroundStation gs = null)
        {
            this.PassID = pass.GroundStationPassID;
            this.ScheduledAos = pass.ScheduledAos;
            this.ScheduledLos = pass.ScheduledLos;
            this.ActualAos = pass.ActualAos;
            this.ActualLos = pass.ActualLos;
            this.ExpectedElevation = pass.ExpectedElevation;
            this.StationName = gs.Name;
        }
    }

    public class ObservableCommand : AbstractNotifyPropertyChanged
    {
        private CommandState m_state;
        private string m_payload;
        private string[] m_history;

        public ObservableCommand(Guid commandID)
        {
            CommandID = commandID;
        }

        public ObservableCommand(Command command)
        {
            this.CommandID = command.CommandID;
            this.Name = command.Name;
            this.DestinationID = command.DestinationID;
            this.Payload = command.RawPayload;
            this.CurrentState = command.CurrentState;
        }


        public Guid CommandID { get; set; }
        public string DestinationID { get; set; }
        public string CommandType { get; set; }
        public int CommandVersion { get; set; }
        public string Name { get; set; }
        public string ExecuteLocation { get; set; }

        public CommandState CurrentState
        {
            get => m_state;
            set => SetAndRaise(ref m_state, value);
        }

        public string Payload
        {
            get => m_payload;
            set => SetAndRaise(ref m_payload, value);
        }

        public string[] History
        {
            set => SetAndRaise(ref m_history, value);
            get => m_history;
        }

        public void CopyPropertiesFrom(ICommand command)
        {
            var srcprops = command.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Where(p => p.CanRead);

            foreach (var dest in this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(p => p.CanWrite))
            {
                var src = srcprops.FirstOrDefault(s => s.Name == dest.Name);
                if (src != null)
                {
                    dest.SetValue(this, src.GetValue(command));
                }
            }
        }
    }
}
