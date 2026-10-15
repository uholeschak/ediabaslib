namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public class PsdzFeatureSpecificFieldCto : IPsdzFeatureSpecificFieldCto
    {
        public int FieldType { get; set; }

        public string FieldValue { get; set; }
    }
}
