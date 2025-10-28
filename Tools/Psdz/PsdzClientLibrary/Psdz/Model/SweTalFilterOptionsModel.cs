using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class SweTalFilterOptionsModel
    {
        [JsonProperty("ta", NullValueHandling = NullValueHandling.Ignore)]
        public TaModel Ta { get; set; }

        [JsonProperty("processClass", NullValueHandling = NullValueHandling.Ignore)]
        public string ProcessClass { get; set; }

        [JsonProperty("sweFilter", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, ActionValues> SweFilter { get; set; }
    }
}