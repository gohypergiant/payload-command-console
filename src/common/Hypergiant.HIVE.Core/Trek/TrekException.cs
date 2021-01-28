using System;

namespace Hypergiant.HIVE
{
    public class TrekException : Exception
    {
        public TrekErrorCode ErrorCode { get; }

        internal TrekException(TrekErrorCode errorCode)
            : base($"A call to the TReK Library returned an error: {errorCode}")
        {
            ErrorCode = errorCode;
        }
    }
}
