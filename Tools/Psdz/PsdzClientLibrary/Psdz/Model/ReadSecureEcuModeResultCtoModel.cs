using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class ReadSecureEcuModeResultCtoModel
    {
        [JsonProperty("failures", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFailureResponseCtoModel> Failures { get; set; }

        [JsonProperty("secureEcuModes", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<KeyValuePairEnumModel<EcuIdentifierModel, SecureEcuModeEto>> SecureEcuModes { get; set; }
    }
}