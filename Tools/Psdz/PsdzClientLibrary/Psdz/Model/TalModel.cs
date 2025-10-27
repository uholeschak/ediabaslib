using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class TalModel
    {
        [JsonProperty("affectedEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuIdentifierModel> AffectedEcus { get; set; }

        [JsonProperty("asXml", NullValueHandling = NullValueHandling.Ignore)]
        public string AsXml { get; set; }

        [JsonProperty("executionTimeType", NullValueHandling = NullValueHandling.Ignore)]
        public ExecutionTimeTypeModel ExecutionTimeType { get; set; }

        [JsonProperty("installedEcuListCurrent", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuIdentifierModel> InstalledEcuListCurrent { get; set; }

        [JsonProperty("installedEcuListTarget", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuIdentifierModel> InstalledEcuListTarget { get; set; }

        [JsonProperty("talElement", NullValueHandling = NullValueHandling.Ignore)]
        public TalElementModel TalElement { get; set; }

        [JsonProperty("talExecutionState", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public TalExecutionStateModel TalExecutionState { get; set; }

        [JsonProperty("talLines", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<TalLineModel> TalLines { get; set; }
    }
}