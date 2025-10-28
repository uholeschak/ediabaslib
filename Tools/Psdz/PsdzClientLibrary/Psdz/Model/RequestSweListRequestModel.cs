using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class RequestSweListRequestModel
    {
        [JsonProperty("ignoreSwDelete", NullValueHandling = NullValueHandling.Ignore)]
        public bool IgnoreSwDelete { get; set; }

        [JsonProperty("tal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel Tal { get; set; }
    }
}