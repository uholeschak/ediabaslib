using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class RequestContextInfoFromVehicleRequestModel
    {
        [JsonProperty("installedEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuIdentifierModel> InstalledEcus { get; set; }

        [JsonProperty("wantedContextItems", NullValueHandling = NullValueHandling.Ignore, ItemConverterType = typeof(StringEnumConverter))]
        public ICollection<EcuContextItemModel> WantedContextItems { get; set; }
    }
}