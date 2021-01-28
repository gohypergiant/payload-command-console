using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hypergiant.HIVE
{
    public class TelemetryRoot : TelemetrySystem
    {
        public TelemetryRoot(params object[] contents)
            : base(null, contents)
        {
        }
    }

    public class TelemetrySystem
    {
        public static TelemetrySystem FromJson(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            options.Converters.Add(new JsonValueConverterTelemetrySystem());
            options.Converters.Add(new JsonValueConverterTelemetryDataItem());

            return JsonSerializer.Deserialize<TelemetrySystem>(json, options);
        }

        internal TelemetrySystem()
        {
            Systems = new TelemetrySystemCollection(this);
            Items = new TelemetryDataItemCollection(this);
        }

        public TelemetrySystem(string id)
            : this(id, null)
        {
            ID = id;
        }

        public TelemetrySystem(string id, params object[] contents)
            : this()
        {
            ID = id;

            if (contents != null)
            {
                foreach (var o in contents)
                {
                    var ts = o as TelemetrySystem;
                    var tdi = o as TelemetryDataItem;

                    if (ts != null)
                    {
                        ts.Parent = this;
                        Systems.Add(ts);
                    }
                    else if (tdi != null)
                    {
                        tdi.Parent = this;
                        Items.Add(tdi);
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid content type: {o.GetType().Name}");
                    }
                }
            }
        }

        [JsonIgnore]
        public TelemetryDataItem this[string path]
        {
            get
            {
                if (!path.StartsWith(this.Path, true, null))
                {
                    // not even in our path - don't bother even looking
                    return null;
                }

                var targetPathSegments = path.Split("/", System.StringSplitOptions.RemoveEmptyEntries);

                var s = this;

                // start at the second item, as the first it the system passed in
                for (int i = 0; i < targetPathSegments.Length; i++)
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
        }

        public TelemetrySystemCollection Systems { get; }
        public TelemetryDataItemCollection Items { get; }
        [JsonIgnore]
        public TelemetrySystem Parent { get; internal set; }

        public string ID { get; set; }
        public string Mnemonic { get; set; }

        public string Path
        {
            get
            {
                if (ID == null)
                {
                    return string.Empty;
                }

                if (Parent == null)
                {
                    return $"/{ID}";
                }
                return $"{Parent.Path}/{ID}";
            }
        }
    }
}