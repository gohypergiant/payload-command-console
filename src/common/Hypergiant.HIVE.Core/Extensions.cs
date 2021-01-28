using System.Text.Json;
using System;

namespace Hypergiant.HIVE
{
    public static class Extensions
    {
        private static JsonSerializerOptions m_options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            IgnoreNullValues = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string ToJson(this TelemetrySystem system)
        {
            return JsonSerializer.Serialize(system, m_options);
        }

        public static string ToJson(this TelemetryDataValue value)
        {
            return JsonSerializer.Serialize(value, m_options);
        }

        public static TelemetryDataItem FindDataItem(this TelemetrySystem system, string path)
        {
            if (!path.StartsWith(system.Path, true, null))
            {
                // not even in our path - don't bother even looking
                return null;
            }

            var targetPathSegments = path.Split("/", System.StringSplitOptions.RemoveEmptyEntries);

            var s = system;
            if (s.ID != targetPathSegments[0])
            {
                return null;
            }

            // start at the second item, as the first it the system passed in
            for (int i = 1; i < targetPathSegments.Length; i++)
            {
                if (s == null)
                {
                    return null;
                }

                if (i == targetPathSegments.Length - 1)
                {
                    // the last item is the data item ID
                    return s.Items[targetPathSegments[i]];
                }

                s = s.Systems[targetPathSegments[i]];
            }

            return null;
        }

        public static long ToMillisecondsSinceGPSEpoch(this DateTime dt)
        {
            DateTime GpsEpoch = new DateTime(1980, 1, 6);
            return (long)(dt.Subtract(GpsEpoch)).TotalMilliseconds;
        }
    }
}