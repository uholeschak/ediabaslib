using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SetLogIdRequestModel
    {
        [JsonProperty("logFilePath", NullValueHandling = NullValueHandling.Ignore)]
        public string LogFilePath { get; set; }
    }
}