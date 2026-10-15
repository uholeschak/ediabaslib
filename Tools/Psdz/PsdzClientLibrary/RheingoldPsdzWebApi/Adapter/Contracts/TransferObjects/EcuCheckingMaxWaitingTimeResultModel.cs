using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class EcuCheckingMaxWaitingTimeResultModel
    {
        [JsonProperty("ecuIdentifierModel", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierModel EcuIdentifierModel { get; set; }

        [JsonProperty("maxWaitingTime", NullValueHandling = NullValueHandling.Ignore)]
        public int MaxWaitingTime { get; set; }
    }
}