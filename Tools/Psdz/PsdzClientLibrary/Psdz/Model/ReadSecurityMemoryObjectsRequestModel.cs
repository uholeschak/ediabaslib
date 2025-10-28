using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class ReadSecurityMemoryObjectsRequestModel
    {
        [JsonProperty("ecus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuIdentifierModel> Ecus { get; set; }

        [JsonProperty("securityMemoryObjectType", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public SecurityMemoryObjectTypeEto SecurityMemoryObjectType { get; set; }

        [JsonProperty("svtIst", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel SvtIst { get; set; }
    }
}