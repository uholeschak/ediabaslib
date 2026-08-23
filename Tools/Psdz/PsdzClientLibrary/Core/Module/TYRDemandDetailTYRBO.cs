using System;
using BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2.Enums;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2
{
    public class TYRDemandDetailTYRBO
    {
        public int? estimatedtimeuntilmobilityloss { get; set; }

        public TPosition? position { get; set; }

        public DateTimeOffset? timestampAtDetection { get; set; }

        public int? mileageAtDetection { get; set; }

        public int? pressure { get; set; }

        public int? pressureTarget { get; set; }

        public bool? pressureLossDetected { get; set; }

        public string pressureLossDetectionQualifier { get; set; }

        public TYRWDemandDetailTYRWBO RR { get; set; }

        public TYRWDemandDetailTYRWBO RL { get; set; }

        public TYRWDemandDetailTYRWBO FR { get; set; }

        public TYRWDemandDetailTYRWBO FL { get; set; }

        public TSeverity? severity { get; set; }
    }
}
