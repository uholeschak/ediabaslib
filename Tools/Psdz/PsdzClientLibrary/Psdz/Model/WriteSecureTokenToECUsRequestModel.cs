using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class WriteSecureTokenToECUsRequestModel
    {
        [JsonProperty("secureToken", NullValueHandling = NullValueHandling.Ignore)]
        public SecureTokenForVehicleEtoModel SecureToken { get; set; }

        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }

        [JsonProperty("ecus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuIdentifierModel> Ecus { get; set; }
    }
}