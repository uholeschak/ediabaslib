using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class ExecuteTalAsyncRequestModel
    {
        [JsonProperty("faTarget", NullValueHandling = NullValueHandling.Ignore)]
        public FaModel FaTarget { get; set; }

        [JsonProperty("svtTarget", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel SvtTarget { get; set; }

        [JsonProperty("tal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel Tal { get; set; }

        [JsonProperty("talExecutionConfig", NullValueHandling = NullValueHandling.Ignore)]
        public TalExecutionConfigModel TalExecutionConfig { get; set; }

        [JsonProperty("vin", NullValueHandling = NullValueHandling.Ignore)]
        public VinModel Vin { get; set; }
    }
}