namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzFeatureSpecificFieldCto
    {
        int FieldType { get; set; }

        string FieldValue { get; set; }
    }
}
