using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hypergiant.HIVE
{
    public class TelemetryDataItemCollection : IEnumerable<TelemetryDataItem>
    {
        private TelemetrySystem Container { get; }
        private List<TelemetryDataItem> m_items = new List<TelemetryDataItem>();
        private object m_syncRoot = new object();

        internal TelemetryDataItemCollection(TelemetrySystem container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            Container = container;
        }

        internal void Add(TelemetryDataItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            lock (m_syncRoot)
            {
                item.Parent = Container;
                m_items.Add(item);
            }
        }

        public int Count
        {
            get
            {
                lock (m_syncRoot)
                {
                    return m_items.Count;
                }
            }
        }

        public TelemetryDataItem this[int index]
        {
            get
            {
                lock (m_syncRoot)
                {
                    return m_items[index];
                }
            }
        }

        public TelemetryDataItem this[string id]
        {
            get
            {
                lock (m_syncRoot)
                {
                    return m_items.FirstOrDefault(i => string.Compare(i.SystemIdentifier, id, true) == 0);
                }
            }
        }

        public IEnumerator<TelemetryDataItem> GetEnumerator()
        {
            lock (m_syncRoot)
            {
                return m_items.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
