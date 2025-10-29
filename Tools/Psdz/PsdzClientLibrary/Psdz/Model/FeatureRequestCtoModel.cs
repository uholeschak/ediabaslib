using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class FeatureRequestCtoModel
    {
        [JsonProperty("ecuUid", NullValueHandling = NullValueHandling.Ignore)]
        public EcuUidCtoModel EcuUid { get; set; }

        [JsonProperty("enableType", NullValueHandling = NullValueHandling.Ignore)]
        public int EnableType { get; set; }

        [JsonProperty("featureId", NullValueHandling = NullValueHandling.Ignore)]
        public FeatureIdCtoModel FeatureId { get; set; }

        [JsonProperty("featureSpecificFields", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<FeatureSpecificFieldCtoModel> FeatureSpecificFields { get; set; }

        [JsonProperty("linkType", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public SfaLinkTypeEto LinkType { get; set; }

        [JsonProperty("validityConditions", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<ValidityConditionCtoModel> ValidityConditions { get; set; }
    }
}