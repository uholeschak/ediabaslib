using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class WriteSvtRequestModel
    {
        [JsonProperty("standardSvt", NullValueHandling = NullValueHandling.Ignore)]
        public StandardSvtModel StandardSvt { get; set; }
    }
}