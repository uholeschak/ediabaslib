using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class EcuDetailInfoModel
    {
        [JsonProperty("byteValue", NullValueHandling = NullValueHandling.Ignore)]
        public byte ByteValue { get; set; }
    }
}