using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class DefineFilterForSwesRequestModel
    {
        [JsonProperty("diagAddress", NullValueHandling = NullValueHandling.Ignore)]
        public int DiagAddress { get; set; }

        [JsonProperty("filter", NullValueHandling = NullValueHandling.Ignore)]
        public TalFilterModel Filter { get; set; }

        [JsonProperty("taCategory", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public TACategories TaCategory { get; set; }

        [JsonProperty("talfilterAction", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ActionValues TalfilterAction { get; set; }

        [JsonProperty("sweFilter", NullValueHandling = NullValueHandling.Ignore)]
        public IList<SweTalFilterOptionsModel> SweFilter { get; set; }
    }
}