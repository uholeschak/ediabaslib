namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public class PsdzValidityConditionCto : IPsdzValidityConditionCto
    {
        public PsdzConditionTypeEtoEnum ConditionType { get; set; }

        public string ValidityValue { get; set; }
    }
}
