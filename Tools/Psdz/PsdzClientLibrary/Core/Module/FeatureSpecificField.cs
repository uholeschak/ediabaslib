using PsdzClient.Core;
using PsdzClient.Programming;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public class FeatureSpecificField : IFeatureSpecificField
    {
        public int FieldType { get; set; }

        public string FieldValue { get; set; }

        public FeatureSpecificField(int fieldType, string fieldValue)
        {
            FieldType = fieldType;
            FieldValue = fieldValue;
        }

        public FeatureSpecificField()
        {
        }
    }
}
