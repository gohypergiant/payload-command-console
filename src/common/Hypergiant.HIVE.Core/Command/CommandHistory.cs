using System;
using System.ComponentModel.Design;

namespace Hypergiant.HIVE
{
    public class CommandHistory
    {
        public CommandHistory(Guid commandID)
        {
            Timestamp = DateTime.UtcNow;
            CommandID = commandID;
        }

        public CommandHistory()
        {
            Timestamp = DateTime.UtcNow;
        }

        public Guid CommandID { get; set; }
        public DateTime Timestamp { get; set; }
        public CommandState? PreviousState { get; set; }
        public CommandState? NewState { get; set; }
        public string Data { get; set; }
    }
}
