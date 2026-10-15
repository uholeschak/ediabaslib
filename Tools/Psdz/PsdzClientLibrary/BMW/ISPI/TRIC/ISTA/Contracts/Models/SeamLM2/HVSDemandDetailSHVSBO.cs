using System.Collections.Generic;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2
{
    public class HVSDemandDetailSHVSBO
    {
        public List<int> moduleList { get; set; }

        public string filterId { get; set; }

        public string isarServiceDemandRef { get; set; }
    }
}
