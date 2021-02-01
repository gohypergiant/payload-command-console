using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hypergiant.HIVE.HGSExecutive
{
    public static class JsonElementExtensions
    {
        public static int GetInt32Property(this JsonElement e, string name, int defaultValue)
        {
            try
            {
                if (e.TryGetProperty(name, out JsonElement prop))
                {
                    if (prop.TryGetInt32(out int i))
                    {
                        return i;
                    }
                }
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
        public static string GetStringProperty(this JsonElement e, string name, string defaultValue)
        {
            try
            {
                if (e.TryGetProperty(name, out JsonElement prop))
                {
                    return prop.GetString();
                }
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    public interface IGroundCommand
    {
        Guid CommandID { get; }
        Task<IEnumerable<CommandHistory>> Execute(ISatelliteCommsService commsService);
    }

    public interface ICommandExecutive
    {
        Task<IEnumerable<CommandHistory>> ExecuteOrDeliver(HGSCommandEnvelope command);
    }
}
