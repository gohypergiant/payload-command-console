using System;
using System.Diagnostics;

namespace Hypergiant
{
    public class GenericEventArgs<T> : EventArgs
    {
        [DebuggerStepThrough]
        public GenericEventArgs(T value)
        {
            Value = value;
        }

        public T Value { get; set; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}