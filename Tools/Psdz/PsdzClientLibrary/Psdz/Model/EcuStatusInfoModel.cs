using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class EcuStatusInfoModel
    {
        [JsonProperty("byteValue", NullValueHandling = NullValueHandling.Ignore)]
        public byte ByteValue { get; set; }

        [JsonProperty("hasIndividualData", NullValueHandling = NullValueHandling.Ignore)]
        public bool HasIndividualData { get; set; }
    }
}