using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SetRootDirectoryRequestModel
    {
        [JsonProperty("rootDirectoryPath", NullValueHandling = NullValueHandling.Ignore)]
        public string RootDirectoryPath { get; set; }
    }
}