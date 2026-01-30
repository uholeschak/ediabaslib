namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public interface IPsdzValidityConditionCto
    {
        PsdzConditionTypeEtoEnum ConditionType { get; set; }

        string ValidityValue { get; set; }
    }
}
