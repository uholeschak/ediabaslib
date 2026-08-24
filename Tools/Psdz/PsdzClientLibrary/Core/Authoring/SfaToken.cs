using PsdzClient.Core;

namespace BMW.Authoring.API.Implementation.Sfa.Models
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public class SfaToken : SfaStatusInfo
    {
        public string TokenId { get; set; }

        public string TokenRaw { get; set; }

        public string SigningCert { get; set; }
    }
}
