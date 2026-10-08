using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
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