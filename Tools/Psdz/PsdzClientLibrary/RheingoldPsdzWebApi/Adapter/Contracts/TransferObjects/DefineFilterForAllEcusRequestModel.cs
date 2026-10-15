using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class DefineFilterForAllEcusRequestModel
    {
        [JsonProperty("inputTalFilter", NullValueHandling = NullValueHandling.Ignore)]
        public TalFilterModel InputTalFilter { get; set; }

        [JsonProperty("taCategories", NullValueHandling = NullValueHandling.Ignore, ItemConverterType = typeof(StringEnumConverter))]
        public ICollection<TACategories> TaCategories { get; set; }

        [JsonProperty("talfilterAction", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ActionValues TalfilterAction { get; set; }
    }
}