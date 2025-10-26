using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    internal class GenericApiResponse<T>
    {
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public T Data { get; set; }
    }
}