using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    [JsonConverter(typeof(JsonInheritanceConverter), new object[] { "discriminatorType" })]
    [JsonInheritance("FaModel", typeof(FaModel))]
    public class StandardFaModel
    {
        [JsonProperty("asString", NullValueHandling = NullValueHandling.Ignore)]
        public string AsString { get; set; }

        [JsonProperty("entwicklungsbaureihe", NullValueHandling = NullValueHandling.Ignore)]
        public string Entwicklungsbaureihe { get; set; }

        [JsonProperty("ewords", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<string> Ewords { get; set; }

        [JsonProperty("faVersion", NullValueHandling = NullValueHandling.Ignore)]
        public int FaVersion { get; set; }

        [JsonProperty("howords", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<string> Howords { get; set; }

        [JsonProperty("lackcode", NullValueHandling = NullValueHandling.Ignore)]
        public string Lackcode { get; set; }

        [JsonProperty("polstercode", NullValueHandling = NullValueHandling.Ignore)]
        public string Polstercode { get; set; }

        [JsonProperty("salapas", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<string> Salapas { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("isValid", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsValid { get; set; }

        [JsonProperty("vin", NullValueHandling = NullValueHandling.Ignore)]
        public string Vin { get; set; }

        [JsonProperty("zeitkriterium", NullValueHandling = NullValueHandling.Ignore)]
        public string Zeitkriterium { get; set; }
    }
}