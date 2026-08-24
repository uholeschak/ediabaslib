using PsdzClient.Core;

namespace BMW.Authoring.API.Implementation.Sfa.Models
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public class SfaStatusInfo : SfaFeatureBase
    {
        public int StatusCode { get; set; }
    }
}
