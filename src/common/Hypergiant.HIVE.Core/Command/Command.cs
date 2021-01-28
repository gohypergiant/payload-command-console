using Hypergiant.HIVE.Support;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;

namespace Hypergiant.HIVE
{
    public class GroundStation
    {
        [Key]
        public Guid GroundStationID { get; set; }
        public string Name { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }

    public class GroundStationPass
    {
        [Key]
        public Guid GroundStationPassID { get; set; }
        public Guid GroundStationID { get; set; }
        public DateTime ScheduledAos { get; set; }
        public DateTime? ScheduledLos { get; set; }
        public decimal? ExpectedElevation { get; set; }
        public DateTime? ActualAos { get; set; }
        public DateTime? ActualLos { get; set; }
    }

    public class GroundStationPassHistoryRecord
    {
        [Key]
        public Guid RecordID { get; set; }
        public Guid GroundStationPassID { get; set; }
        public Guid SatelliteID { get; set; }
        public Guid? UserID { get; set; }
        public DateTime TimestampUtc { get; set; }
        public PassAction Action { get; set; }
        public HistoryDetailType DetailType { get; set; }
        public string Summary { get; set; }
        public string Detail { get; set; }
    }

    public enum HistoryDetailType
    {
        Text,
        Image,
        Other
    }

    public enum PassAction
    {
        AOS,
        GoForCommand,
        Note,
        LOS
    }

    public class Command : ICommand
    {
        [Key]
        public Guid CommandID { get; set; }
        public Guid GroundStationPassID { get; set; }
        public Guid? ParentCommandID { get; set; }
        public Guid? SiblingCommandID { get; set; }
        public string Name { get; set; }
        public CommandState CurrentState { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Mnemonic { get; set; }
        public CommandPriority Priority { get; set; }
        public bool RequiresApproval { get; set; }
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string SourceID { get; set; }
        public string DestinationID { get; set; }
        public string RawPayload { get; set; }
        public string ExecuteLocation { get; set; }

        public Command()
        {

        }

        public static ICommand Create(ICommandEnvelope envelope)
        {
            if (envelope.CommandID == Guid.Empty)
            {
                envelope.CommandID = Guid.NewGuid();
            }

            return new Command
            {
                CommandID = envelope.CommandID,
                ExecuteLocation = envelope.ExecuteLocation,
                Name = envelope.Name,
                RawPayload = envelope.ToRawPayload()
            };
        }

        protected void CopyPropertiesFrom(ICommand command)
        {
            var srcprops = command.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Where(p => p.CanRead);

            foreach (var dest in typeof(Command).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(p => p.CanWrite))
            {
                var src = srcprops.FirstOrDefault(s => s.Name == dest.Name);
                if (src != null)
                {
                    dest.SetValue(this, src);
                }
            }
        }

        public static Command From(ICommand command)
        {
            var result = new Command();
            result.CopyPropertiesFrom(command);
            return result;
        }

    }

    public class HGSCommand : HGSCommandEnvelope, ICommand
    {
        public Guid GroundStationPassID { get; set; }
        public Guid? ParentCommandID { get; set; }
        public Guid? SiblingCommandID { get; set; }
        public CommandState CurrentState { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Mnemonic { get; set; }
        public CommandPriority Priority { get; set; }
        public bool RequiresApproval { get; set; }
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string SourceID { get; set; }
        public string DestinationID { get; set; }
        public string RawPayload { get; set; }
    }

    public interface ICommandEnvelope
    {
        Guid CommandID { get; set; }
        string Name { get; set; }
        string ExecuteLocation { get; }
        System.Text.Json.JsonElement ExecutionData { get; }
        string ToRawPayload();
    }

    public class HGSCommandEnvelope : ICommandEnvelope
    {
        private static JsonSerializerOptions m_options;

        static HGSCommandEnvelope()
        {
            m_options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true
            };
        }

        [Key]
        public Guid CommandID { get; set; }
        public string CommandType { get; set; }
        public int CommandVersion { get; set; }
        public string Name { get; set; }
        public string ExecuteLocation { get; set; }
        public System.Text.Json.JsonElement ExecutionData { get; set; }

        public string ToRawPayload()
        {
            return JsonSerializer.Serialize(this, m_options);
        }

        public static HGSCommandEnvelope Parse(string commandContent)
        {
            // is the content JSON
            var envelope = JsonSerializer.Deserialize<HGSCommandEnvelope>(commandContent, m_options);
            if (envelope.CommandType != "HGS")
            {
                // it is not, assume it's just a JSON satellite command (old-style) so package it up
                var json = JsonDocument.Parse(commandContent);
                envelope = new HGSCommandEnvelope();
                envelope.CommandType = "HGS";
                envelope.CommandVersion = 1;
                envelope.Name = "[Unnamed]";
                envelope.ExecuteLocation = "satellite";
                envelope.ExecutionData = json.RootElement;
            }

            envelope.CommandID = Guid.NewGuid();

            return envelope;
        }

        public static HGSCommandEnvelope Create<T>(string name, bool executeOnSatellite, T executionData)
        {
            var json = JsonSerializer.Serialize(executionData, m_options);

            var envelope = new HGSCommandEnvelope
            {
                CommandID = Guid.NewGuid(),
                CommandType = "HGS",
                CommandVersion = 1,
                Name = name,
                ExecuteLocation = executeOnSatellite ? "satellite" : "ground",
                ExecutionData = JsonDocument.Parse(json).RootElement
            };
            return envelope;
        }
    }
}
