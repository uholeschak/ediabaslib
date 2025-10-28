using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    [JsonConverter(typeof(JsonInheritanceConverter), new object[] { "discriminatorType" })]
    [JsonInheritance("EcuVariantInstanceModel", typeof(EcuVariantInstanceModel))]
    [JsonInheritance("OrderPartModel", typeof(OrderPartModel))]
    [JsonInheritance("ReplacementPartModel", typeof(ReplacementPartModel))]
    public class LogisticPartModel
    {
        [JsonProperty("nameTais", NullValueHandling = NullValueHandling.Ignore)]
        public string NameTais { get; set; }

        [JsonProperty("sachNrTais", NullValueHandling = NullValueHandling.Ignore)]
        public string SachNrTais { get; set; }

        [JsonProperty("typ", NullValueHandling = NullValueHandling.Ignore)]
        public int Typ { get; set; }

        [JsonProperty("zusatzTextRef", NullValueHandling = NullValueHandling.Ignore)]
        public string ZusatzTextRef { get; set; }
    }
}