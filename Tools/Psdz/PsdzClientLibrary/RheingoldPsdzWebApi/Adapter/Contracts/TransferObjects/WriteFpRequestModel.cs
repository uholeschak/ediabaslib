using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class WriteFpRequestModel
    {
        [JsonProperty("fpAsString", NullValueHandling = NullValueHandling.Ignore)]
        public string FpAsString { get; set; }
    }
}