using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PsdzClient.Core
{
    public class PropertyData<T>
    {
        public T Value { get; set; }
        public bool IsValidValue { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
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