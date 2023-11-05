using System;

namespace PsdzClientLibrary.Core
{
    public class PropertyData<T>
    {
        public T Value { get; set; }

        public bool IsValidValue { get; set; }

        public DataSource Source { get; set; }

        public DateTime Timestamp { get; set; }

        public PropertyData()
        {
        }

        public PropertyData(T value, DataSource source)
        {
            Value = value;
            Source = source;
            Timestamp = DateTime.Now;
            IsValidValue = true;
        }
    }
}
