using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class ReadNcdFromFileRequestModel
    {
        [JsonProperty("btldSgbmNumber", NullValueHandling = NullValueHandling.Ignore)]
        public string BtldSgbmNumber { get; set; }

        [JsonProperty("cafdSgbmId", NullValueHandling = NullValueHandling.Ignore)]
        public SgbmIdModel CafdSgbmId { get; set; }

        [JsonProperty("ncdDirectoryPath", NullValueHandling = NullValueHandling.Ignore)]
        public string NcdDirectoryPath { get; set; }

        [JsonProperty("vin", NullValueHandling = NullValueHandling.Ignore)]
        public VinModel Vin { get; set; }
    }
}