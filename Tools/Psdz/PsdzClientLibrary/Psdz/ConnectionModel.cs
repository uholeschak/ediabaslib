using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System;

namespace BMW.Rheingold.Psdz
{
    public class ConnectionModel
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid Id { get; set; }

        [JsonProperty("port", NullValueHandling = NullValueHandling.Ignore)]
        public int Port { get; set; }

        [JsonProperty("targetSelector", NullValueHandling = NullValueHandling.Ignore)]
        public TargetSelectorModel TargetSelector { get; set; }
    }
}