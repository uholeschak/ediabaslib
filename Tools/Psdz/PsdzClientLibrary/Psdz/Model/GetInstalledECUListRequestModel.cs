using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class GetInstalledECUListRequestModel
    {
        [JsonProperty("blacklisted", NullValueHandling = NullValueHandling.Ignore)]
        public bool Blacklisted { get; set; }

        [JsonProperty("diagAddressModels", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<DiagAddressModel> DiagAddressModels { get; set; }

        [JsonProperty("fa", NullValueHandling = NullValueHandling.Ignore)]
        public FaModel Fa { get; set; }

        [JsonProperty("ilevel", NullValueHandling = NullValueHandling.Ignore)]
        public string Ilevel { get; set; }
    }
}