using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    [JsonConverter(typeof(JsonInheritanceConverter), new object[] { "discriminatorType" })]
    [JsonInheritance("FpModel", typeof(FpModel))]
    public class StandardFpModel
    {
        [JsonProperty("asString", NullValueHandling = NullValueHandling.Ignore)]
        public string AsString { get; set; }

        [JsonProperty("category2Criteria", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, ICollection<StandardFpCriterionModel>> Category2Criteria { get; set; }

        [JsonProperty("categoryId2CategoryName", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, string> CategoryId2CategoryName { get; set; }

        [JsonProperty("isValid", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsValid { get; set; }
    }
}