using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hypergiant.HIVE
{
    public class TelemetrySystemCollection : IEnumerable<TelemetrySystem>
    {
        private TelemetrySystem Container { get; }
        private List<TelemetrySystem> m_systems = new List<TelemetrySystem>();
        private object m_syncRoot = new object();

        internal TelemetrySystemCollection(TelemetrySystem container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            Container = container;
        }

        internal void Add(TelemetrySystem system)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            lock (m_syncRoot)
            {
                system.Parent = Container;
                m_systems.Add(system);
            }
        }

        public int Count
        {
            get
            {
                lock (m_syncRoot)
                {
                    return m_systems.Count;
                }
            }
        }

        public TelemetrySystem this[int index]
        {
            get
            {
                lock (m_syncRoot)
                {
                    return m_systems[index];
                }
            }
        }

        public TelemetrySystem this[string id]
        {
            get
            {
                lock (m_syncRoot)
                {
                    return m_systems.FirstOrDefault(s => string.Compare(s.ID, id, true) == 0);
                }
            }
        }

        public IEnumerator<TelemetrySystem> GetEnumerator()
        {
            return m_systems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}