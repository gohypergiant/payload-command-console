using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hypergiant.HIVE
{
    public class PublisherFilterStack
    {
        private List<ITelemetryPublisher> m_publishers = new List<ITelemetryPublisher>();

        // TODO: right now all publishers run in parallel.  Add ability to serialize (parent/child relationship on publisher, maybe - like a linked list)?
        public void Add(ITelemetryPublisher publisher)
        {
            if (publisher == null)
            {
                throw new ArgumentNullException(nameof(publisher));
            }

            lock (m_publishers)
            {
                m_publishers.Add(publisher);
            }
        }

        public int Count
        {
            get
            {
                lock (m_publishers)
                {
                    return m_publishers.Count;
                }
            }
        }

        public async Task Publish(TelemetryDataValue value)
        {
            var tasks = m_publishers.Select(async p =>
            {
                p.QueueTelemetry("/", value);
            });
            await Task.WhenAll(tasks);
        }
    }
}
