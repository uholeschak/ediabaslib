using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class ProgrammingProtectionDataCtoModel
    {
        [JsonProperty("programmingProtectionEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuIdentifierModel> ProgrammingProtectionEcus { get; set; }

        [JsonProperty("sweList", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SgbmIdModel> SweList { get; set; }

        [JsonProperty("sweData", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] SweData { get; set; }
    }
}