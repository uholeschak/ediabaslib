using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
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