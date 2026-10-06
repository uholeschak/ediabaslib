using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
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