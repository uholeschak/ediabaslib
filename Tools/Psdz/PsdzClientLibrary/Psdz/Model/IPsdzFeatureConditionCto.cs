namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public interface IPsdzFeatureConditionCto
    {
        PsdzConditionTypeEtoEnum ConditionType { get; set; }

        string CurrentValidityValue { get; set; }

        int Length { get; set; }

        string ValidityValue { get; set; }
    }
}
