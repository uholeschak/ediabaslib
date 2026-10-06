using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
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