using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class IdLightTaModel : TaModel
    {
        [JsonProperty("idLightTaType", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public IdLightTaTypeModel IdLightTaType { get; set; }

        [JsonProperty("ids", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<string> Ids { get; set; }
    }
}