using BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2.Enums;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2
{
    public sealed class BATDemandDetailBATBO
    {
        public BatteryType? batteryType { get; set; }

        public double? wearAgeComponent { get; set; }

        public int? wearLifeTimePercentage { get; set; }

        public int? travelledDistanceAtDetection { get; set; }

        public int? capacity { get; set; }

        public int? healthClass { get; set; }

        public int? vdaHealthClass { get; set; }

        public int? customerHealthClass { get; set; }

        public int? startHealthClass { get; set; }

        public int? stateOfChargeHealthClass { get; set; }
    }
}
