using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Rheingold.Psdz
{
    public class DetailedStatusCtoModel
    {
        [JsonProperty("diagAddressCtoModel", NullValueHandling = NullValueHandling.Ignore)]
        public DiagAddressCtoModel DiagAddressCtoModel { get; set; }

        [JsonProperty("featureIdCtoModel", NullValueHandling = NullValueHandling.Ignore)]
        public FeatureIdCtoModel FeatureIdCtoModel { get; set; }

        [JsonProperty("tokenDetailedStatusEto", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public TokenDetailedStatusEto TokenDetailedStatusEto { get; set; }
    }
}