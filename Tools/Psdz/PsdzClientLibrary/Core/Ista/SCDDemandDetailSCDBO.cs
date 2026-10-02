using System.Collections.Generic;
using BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2.Enums;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2
{
    public class SCDDemandDetailSCDBO
    {
        public string filterId { get; set; }

        public double? filterVersion { get; set; }

        public List<int> causingCcms { get; set; }

        public double? accuracy { get; set; }

        public string isarDocumentId { get; set; }

        public SCDDataSource? dataSource { get; set; }
    }
}
