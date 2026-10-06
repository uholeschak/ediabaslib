using BMW.Rheingold.CoreFramework;

namespace PsdzClient.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IFeatureSpecificField
    {
        int FieldType { get; set; }

        string FieldValue { get; set; }
    }
}
