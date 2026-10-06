using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using PsdzClient.Psdz.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class EcuResetMapping
    {
        [JsonProperty("ecu", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierCtoModel Ecu { get; set; }

        [JsonProperty("resetType", NullValueHandling = NullValueHandling.Ignore)]
        public ResetTypeEto ResetType { get; set; }
    }
}