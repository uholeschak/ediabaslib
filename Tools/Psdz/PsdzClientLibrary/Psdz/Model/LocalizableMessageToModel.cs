using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class LocalizableMessageToModel
    {
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("messageId", NullValueHandling = NullValueHandling.Ignore)]
        public int MessageId { get; set; }
    }
}