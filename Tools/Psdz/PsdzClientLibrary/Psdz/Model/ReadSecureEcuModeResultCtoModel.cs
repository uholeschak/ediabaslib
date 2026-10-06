using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

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