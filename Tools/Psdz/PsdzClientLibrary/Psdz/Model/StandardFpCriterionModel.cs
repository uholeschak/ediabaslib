using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class StandardFpCriterionModel
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("nameEn", NullValueHandling = NullValueHandling.Ignore)]
        public string NameEn { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public int Value { get; set; }
    }
}