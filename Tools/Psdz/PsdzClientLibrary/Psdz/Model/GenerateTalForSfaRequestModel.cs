using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class GenerateTalForSfaRequestModel
    {
        [JsonProperty("calculationStrategy", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public CalculationStrategyEto CalculationStrategy { get; set; }

        [JsonProperty("diagAdresses", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<DiagAddressModel> DiagAdresses { get; set; }

        [JsonProperty("featureActivationTokens", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SecureTokenEtoModel> FeatureActivationTokens { get; set; }

        [JsonProperty("featureIdBlackList", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<FeatureIdCtoModel> FeatureIdBlackList { get; set; }

        [JsonProperty("featureIdWhiteList", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<FeatureIdCtoModel> FeatureIdWhiteList { get; set; }

        [JsonProperty("sfaCurrent", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<FeatureLongStatusCtoModel> SfaCurrent { get; set; }

        [JsonProperty("sfaTarget", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFeatureTokenRelationCtoModel> SfaTarget { get; set; }

        [JsonProperty("supressCreationOfSfaWriteTA", NullValueHandling = NullValueHandling.Ignore)]
        public bool SupressCreationOfSfaWriteTA { get; set; }

        [JsonProperty("tal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel Tal { get; set; }
    }
}