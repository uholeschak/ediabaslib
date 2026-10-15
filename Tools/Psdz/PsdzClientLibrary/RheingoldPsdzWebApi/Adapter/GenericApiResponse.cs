using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter
{
    internal class GenericApiResponse<T>
    {
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public T Data { get; set; }
    }
}