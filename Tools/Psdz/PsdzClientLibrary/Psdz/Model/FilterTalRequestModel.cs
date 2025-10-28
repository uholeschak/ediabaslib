using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class FilterTalRequestModel
    {
        [JsonProperty("tal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel Tal { get; set; }

        [JsonProperty("talFilter", NullValueHandling = NullValueHandling.Ignore)]
        public TalFilterModel TalFilter { get; set; }
    }
}