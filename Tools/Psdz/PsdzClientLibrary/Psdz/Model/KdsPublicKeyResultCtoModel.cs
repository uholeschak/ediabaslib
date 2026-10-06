using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class KdsPublicKeyResultCtoModel
    {
        [JsonProperty("kdsIdCto", NullValueHandling = NullValueHandling.Ignore)]
        public KdsIdCtoModel KdsIdCto { get; set; }

        [JsonProperty("publicKey", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] PublicKey { get; set; }

        [JsonProperty("resultState", NullValueHandling = NullValueHandling.Ignore)]
        public int ResultState { get; set; }

        [JsonProperty("ecuUid", NullValueHandling = NullValueHandling.Ignore)]
        public string EcuUid { get; set; }
    }
}