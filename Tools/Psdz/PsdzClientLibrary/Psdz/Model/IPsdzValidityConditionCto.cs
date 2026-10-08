using BMW.Rheingold.Psdz.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzValidityConditionCto
    {
        PsdzConditionTypeEtoEnum ConditionType { get; set; }

        string ValidityValue { get; set; }
    }
}
