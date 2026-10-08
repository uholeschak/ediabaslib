using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class EventListeningRequestModel
    {
        [JsonProperty("psdZEventTypes", NullValueHandling = NullValueHandling.Ignore, ItemConverterType = typeof(StringEnumConverter))]
        public ICollection<PSdZEventType> PsdZEventTypes { get; set; }
    }
}