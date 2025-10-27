using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class ImportPdxRequestModel
    {
        [JsonProperty("pathToPdxContainer", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<string> PathToPdxContainer { get; set; }

        [JsonProperty("projectName", NullValueHandling = NullValueHandling.Ignore)]
        public string ProjectName { get; set; }

        [JsonProperty("rootDirectory", NullValueHandling = NullValueHandling.Ignore)]
        public string RootDirectory { get; set; }
    }
}