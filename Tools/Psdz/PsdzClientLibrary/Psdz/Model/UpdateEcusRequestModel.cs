using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class UpdateEcusRequestModel
    {
        [JsonProperty("installedEcuListIst", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuIdentifierModel> InstalledEcuListIst { get; set; }

        [JsonProperty("installedEcuListSoll", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuIdentifierModel> InstalledEcuListSoll { get; set; }

        [JsonProperty("tal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel Tal { get; set; }
    }
}