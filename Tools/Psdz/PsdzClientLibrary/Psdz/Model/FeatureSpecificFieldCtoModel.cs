using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class FeatureSpecificFieldCtoModel
    {
        [JsonProperty("fieldType", NullValueHandling = NullValueHandling.Ignore)]
        public int FieldType { get; set; }

        [JsonProperty("fieldValue", NullValueHandling = NullValueHandling.Ignore)]
        public string FieldValue { get; set; }
    }
}