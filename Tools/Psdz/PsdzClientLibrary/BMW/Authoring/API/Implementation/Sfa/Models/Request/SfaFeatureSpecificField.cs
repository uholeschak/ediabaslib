using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.API.Implementation.Sfa.Models.Request
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public class SfaFeatureSpecificField
    {
        public long Tag { get; set; }
        public string Value { get; set; }
    }
}