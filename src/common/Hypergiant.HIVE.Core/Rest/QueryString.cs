using System;
using System.Collections.Generic;
using System.Linq;

namespace Hypergiant.HIVE.Rest
{
    public class QueryString
    {
        public string RawData { get; }
        public Dictionary<string, string> Values { get; } = new Dictionary<string, string>();

        internal QueryString(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return;
            }

            var pairs = raw.Split('&');

            foreach (var pair in pairs)
            {
                var kv = pair.Split('=');
                switch (kv.Length)
                {
                    case 1:
                    case 2:
                        var key = kv[0];
                        var val = (kv.Length == 2) ? kv[1] : null;

                        if (!Values.ContainsKey(key))
                        {
                            Values.Add(key, val);
                        }
                        break;
                }
            }
        }

        public bool ContainsKey(string name, bool ignoreCase = false)
        {
            if (Values.ContainsKey(name))
            {
                return true;
            }

            if (ignoreCase)
            {
                return Values.Keys.Contains(name, StringComparer.InvariantCultureIgnoreCase);
            }
            return false;
        }

        public string Get(string name, bool ignoreCase = false)
        {
            var val = Values.FirstOrDefault(v => string.Compare(v.Key, name, ignoreCase) == 0);
            return val.Key == null ? null : val.Value;
        }

        public string this[string name]
        {
            get
            {
                if (Values.ContainsKey(name))
                {
                    return Values[name];
                }
                return null;
            }
        }
    }
}
