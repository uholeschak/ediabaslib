using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class RequestProgrammingTokensOfflineWithGenericResultRequestModel
    {
        [JsonProperty("vin", NullValueHandling = NullValueHandling.Ignore)]
        public VinModel Vin { get; set; }

        [JsonProperty("tal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel Tal { get; set; }

        [JsonProperty("svtCurrent", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel SvtCurrent { get; set; }

        [JsonProperty("svtTarget", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel SvtTarget { get; set; }

        [JsonProperty("tokenVersion", NullValueHandling = NullValueHandling.Ignore)]
        public int TokenVersion { get; set; }

        [JsonProperty("requestFilePath", NullValueHandling = NullValueHandling.Ignore)]
        public string RequestFilePath { get; set; }
    }
}