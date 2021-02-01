using System.Collections;
using System.Collections.Generic;

namespace Hypergiant.HIVE
{
    public class CfdpTransferRequestCollection : IEnumerable<CfdpTransferRequest>
    {
        private List<CfdpTransferRequest> m_list = new List<CfdpTransferRequest>();

        internal CfdpTransferRequestCollection()
        {
        }

        internal void Add(CfdpTransferRequest request)
        {
            m_list.Add(request);
        }

        internal void Clear()
        {
            m_list.Clear();
        }

        public int Count
        {
            get => m_list.Count;
        }

        public CfdpTransferRequest this[int index]
        {
            get => m_list[index];
        }

        public IEnumerator<CfdpTransferRequest> GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
