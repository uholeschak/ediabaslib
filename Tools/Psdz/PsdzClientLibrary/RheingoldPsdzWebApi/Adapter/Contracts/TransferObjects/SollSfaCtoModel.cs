using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SollSfaCtoModel
    {
        [JsonProperty("sollFeatures", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFeatureTokenRelationCtoModel> SollFeatures { get; set; }
    }
}