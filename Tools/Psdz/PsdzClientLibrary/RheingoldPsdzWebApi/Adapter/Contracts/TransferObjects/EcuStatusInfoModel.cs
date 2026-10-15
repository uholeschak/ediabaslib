using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class EcuStatusInfoModel
    {
        [JsonProperty("byteValue", NullValueHandling = NullValueHandling.Ignore)]
        public byte ByteValue { get; set; }

        [JsonProperty("hasIndividualData", NullValueHandling = NullValueHandling.Ignore)]
        public bool HasIndividualData { get; set; }
    }
}