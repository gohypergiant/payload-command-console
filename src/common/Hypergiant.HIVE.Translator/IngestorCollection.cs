using System;
using System.Collections;
using System.Collections.Generic;

namespace Hypergiant.HIVE
{
    public class IngestorCollection : IEnumerable<IIngestor>
    {
        public event EventHandler<GenericEventArgs<IIngestor>> IngestorAdded;

        private List<IIngestor> m_ingestors = new List<IIngestor>();

        public void Add(IIngestor ingestor)
        {
            m_ingestors.Add(ingestor);
            IngestorAdded?.Invoke(this, new GenericEventArgs<IIngestor>(ingestor));
        }

        public IEnumerator<IIngestor> GetEnumerator()
        {
            lock (m_ingestors)
            {
                return m_ingestors.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get
            {
                lock (m_ingestors)
                {
                    return m_ingestors.Count;
                }
            }
        }
    }
}
