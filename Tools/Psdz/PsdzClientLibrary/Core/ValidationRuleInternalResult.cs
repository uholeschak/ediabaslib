using BMW.ISPI.TRIC.ISTA.Contracts.Enums;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Implementations
{
    public class ValidationRuleInternalResult
    {
        public CharacteristicType Type { get; set; }
        public bool IsValid { get; set; }
        public string Id { get; set; }
        public decimal CharacteristicId { get; set; }
    }
}