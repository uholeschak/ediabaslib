using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class DefineFilterForSelectedEcusRequestModel
    {
        [JsonProperty("diagAddress", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<int> DiagAddress { get; set; }

        [JsonProperty("inputTalFilter", NullValueHandling = NullValueHandling.Ignore)]
        public TalFilterModel InputTalFilter { get; set; }

        [JsonProperty("taCategories", NullValueHandling = NullValueHandling.Ignore, ItemConverterType = typeof(StringEnumConverter))]
        public ICollection<TACategories> TaCategories { get; set; }

        [JsonProperty("talfilterAction", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ActionValues TalfilterAction { get; set; }

        [JsonProperty("smacFilter", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, ActionValues> SmacFilter { get; set; }
    }
}