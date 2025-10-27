using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class EventListeningRequestModel
    {
        [JsonProperty("psdZEventTypes", NullValueHandling = NullValueHandling.Ignore, ItemConverterType = typeof(StringEnumConverter))]
        public ICollection<PSdZEventType> PsdZEventTypes { get; set; }
    }
}