using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class RequestCalculationNcdAndSignatureOfflineRequestModel
    {
        [JsonProperty("fa", NullValueHandling = NullValueHandling.Ignore)]
        public FaModel Fa { get; set; }

        [JsonProperty("jsonFilePath", NullValueHandling = NullValueHandling.Ignore)]
        public string JsonFilePath { get; set; }

        [JsonProperty("secureCodingConfigCto", NullValueHandling = NullValueHandling.Ignore)]
        public SecureCodingConfigCtoModel SecureCodingConfigCto { get; set; }

        [JsonProperty("sgbmIdsForNcdCalculation", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<RequestNcdEtoModel> SgbmIdsForNcdCalculation { get; set; }

        [JsonProperty("vin", NullValueHandling = NullValueHandling.Ignore)]
        public VinModel Vin { get; set; }

        [JsonProperty("vpc", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Vpc { get; set; }
    }
}