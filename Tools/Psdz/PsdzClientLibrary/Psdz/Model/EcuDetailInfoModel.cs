using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class EcuDetailInfoModel
    {
        [JsonProperty("byteValue", NullValueHandling = NullValueHandling.Ignore)]
        public byte ByteValue { get; set; }
    }
}