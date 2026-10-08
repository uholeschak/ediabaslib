using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class ReadSecureEcuModeResultCtoModel
    {
        [JsonProperty("failures", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFailureResponseCtoModel> Failures { get; set; }

        [JsonProperty("secureEcuModes", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<KeyValuePairEnumModel<EcuIdentifierModel, SecureEcuModeEto>> SecureEcuModes { get; set; }
    }
}