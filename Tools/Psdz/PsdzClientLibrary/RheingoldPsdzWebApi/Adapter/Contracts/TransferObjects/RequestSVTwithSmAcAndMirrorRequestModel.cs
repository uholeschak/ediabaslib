using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class RequestSVTwithSmAcAndMirrorRequestModel
    {
        [JsonProperty("installedEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuIdentifierModel> InstalledEcus { get; set; }
    }
}