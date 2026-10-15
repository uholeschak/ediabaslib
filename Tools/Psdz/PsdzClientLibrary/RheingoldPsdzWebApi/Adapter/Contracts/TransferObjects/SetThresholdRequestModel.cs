using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SetThresholdRequestModel
    {
        [JsonProperty("threshold", NullValueHandling = NullValueHandling.Ignore)]
        public int Threshold { get; set; }
    }
}