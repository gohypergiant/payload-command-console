using System.Collections;
using System.Collections.Generic;

namespace Hypergiant.HIVE.Rest
{
    public sealed class HttpHeaders : IEnumerable<KeyValuePair<string, string>>
    {
        private Dictionary<string, string> m_headers = new Dictionary<string, string>();

        // TODO: implement other common headers
        public string ContentType
        {
            get => Get("Content-Type");
            set => Set("Content-Type", value);
        }

        public string this[string name]
        {
            get => Get(name);
            set => Set(name, value);
        }

        public string Get(string name)
        {
            lock (m_headers)
            {
                if (m_headers.ContainsKey(name))
                {
                    return m_headers[name];
                }
                return null;
            }
        }

        public void Set(string name, string value)
        {
            lock (m_headers)
            {
                if (m_headers.ContainsKey(name))
                {
                    m_headers[name] = value;
                }
                else
                {
                    m_headers.Add(name, value);
                }
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return m_headers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
