using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IFeatureSpecificField
    {
        int FieldType { get; set; }

        string FieldValue { get; set; }
    }
}
