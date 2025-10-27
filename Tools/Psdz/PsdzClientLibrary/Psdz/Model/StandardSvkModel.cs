using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class StandardSvkModel
    {
        [JsonProperty("progDepChecked", NullValueHandling = NullValueHandling.Ignore)]
        public byte ProgDepChecked { get; set; }

        [JsonProperty("sgbmIds", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SgbmIdModel> SgbmIds { get; set; }

        [JsonProperty("svkVersion", NullValueHandling = NullValueHandling.Ignore)]
        public byte SvkVersion { get; set; }
    }
}