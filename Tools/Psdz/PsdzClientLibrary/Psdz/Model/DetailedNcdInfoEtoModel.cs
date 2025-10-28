using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class DetailedNcdInfoEtoModel
    {
        [JsonProperty("btld", NullValueHandling = NullValueHandling.Ignore)]
        public SgbmIdModel Btld { get; set; }

        [JsonProperty("cafd", NullValueHandling = NullValueHandling.Ignore)]
        public SgbmIdModel Cafd { get; set; }

        [JsonProperty("codingVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string CodingVersion { get; set; }

        [JsonProperty("diagAddresses", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<DiagAddressCtoModel> DiagAddresses { get; set; }

        [JsonProperty("ncdStatus", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public NcdStatusEto NcdStatus { get; set; }
    }
}