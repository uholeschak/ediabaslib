using BMW.Rheingold.CoreFramework;
using PsdzClient.Core;

namespace PsdzClient.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IFeatureSpecificField
    {
        int FieldType { get; set; }

        string FieldValue { get; set; }
    }
}
