using System.Diagnostics;

namespace HANReader.Core.Models
{
    public abstract class Field
    {
        public abstract DataType DataType { get; }
    }

    [DebuggerDisplay("{Value}")]
    public abstract class Field<T> : Field
    {
        private readonly T value;

        protected Field(T value) => this.value = value;

        public T Value => value;
    }
}


