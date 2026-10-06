using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class ExecuteTalRequestModel
    {
        [JsonProperty("fa", NullValueHandling = NullValueHandling.Ignore)]
        public FaModel Fa { get; set; }

        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }

        [JsonProperty("swtApplicationList", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SwtApplicationModel> SwtApplicationList { get; set; }

        [JsonProperty("tal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel Tal { get; set; }

        [JsonProperty("talExecutionConfig", NullValueHandling = NullValueHandling.Ignore)]
        public TalExecutionConfigModel TalExecutionConfig { get; set; }

        [JsonProperty("vin", NullValueHandling = NullValueHandling.Ignore)]
        public VinModel Vin { get; set; }
    }
}