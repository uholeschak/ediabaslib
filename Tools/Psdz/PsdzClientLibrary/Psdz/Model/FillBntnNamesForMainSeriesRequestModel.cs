using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class FillBntnNamesForMainSeriesRequestModel
    {
        [JsonProperty("baureihenverbund", NullValueHandling = NullValueHandling.Ignore)]
        public string Baureihenverbund { get; set; }

        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public StandardSvtModel Svt { get; set; }
    }
}