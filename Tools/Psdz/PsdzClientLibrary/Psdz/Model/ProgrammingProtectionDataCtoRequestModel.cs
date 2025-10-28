using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class ProgrammingProtectionDataCtoRequestModel
    {
        [JsonProperty("tal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel Tal;
    }
}