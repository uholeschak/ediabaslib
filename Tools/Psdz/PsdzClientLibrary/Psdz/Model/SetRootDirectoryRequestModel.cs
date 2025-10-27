using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class SetRootDirectoryRequestModel
    {
        [JsonProperty("rootDirectoryPath", NullValueHandling = NullValueHandling.Ignore)]
        public string RootDirectoryPath { get; set; }
    }
}