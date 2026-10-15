using System.Collections.Generic;
using BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2.Enums;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2
{
    public sealed class EPMDemandDetailEPMBO
    {
        public string filterId { get; set; }

        public double? filterVersion { get; set; }

        public List<int> causingCcms { get; set; }

        public double? accuracy { get; set; }

        public string isarDocumentId { get; set; }

        public EPMDataSource? dataSource { get; set; }
    }
}
