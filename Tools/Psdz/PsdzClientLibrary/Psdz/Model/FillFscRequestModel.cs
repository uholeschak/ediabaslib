using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class FillFscRequestModel
    {
        [JsonProperty("swtApplications", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SwtApplicationModel> SwtApplications { get; set; }

        [JsonProperty("tal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel Tal { get; set; }
    }
}