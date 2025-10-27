using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class ConnectionSettingsRequestModel
    {
        [JsonProperty("additionalTransmissionTimeout", NullValueHandling = NullValueHandling.Ignore)]
        public int AdditionalTransmissionTimeout { get; set; }

        [JsonProperty("bauIstufe", NullValueHandling = NullValueHandling.Ignore)]
        public string BauIstufe { get; set; }

        [JsonProperty("baureihe", NullValueHandling = NullValueHandling.Ignore)]
        public string Baureihe { get; set; }

        [JsonProperty("busName", NullValueHandling = NullValueHandling.Ignore)]
        public BusNameModel BusName { get; set; }

        [JsonProperty("logLevel", NullValueHandling = NullValueHandling.Ignore)]
        public string LogLevel { get; set; }

        [JsonProperty("project", NullValueHandling = NullValueHandling.Ignore)]
        public string Project { get; set; }

        [JsonProperty("shouldSetLinkPropertiesToDCan", NullValueHandling = NullValueHandling.Ignore)]
        public bool ShouldSetLinkPropertiesToDCan { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("vehicleInfo", NullValueHandling = NullValueHandling.Ignore)]
        public string VehicleInfo { get; set; }

        [JsonProperty("vin", NullValueHandling = NullValueHandling.Ignore)]
        public string Vin { get; set; }

        [JsonProperty("isTlsAllowed", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsTlsAllowed { get; set; }
    }
}