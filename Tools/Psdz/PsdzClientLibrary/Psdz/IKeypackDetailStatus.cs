using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using PsdzClient.Core;
using PsdzClient.Programming;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IKeypackDetailStatus
    {
        EcuCertCheckingStatus? KeyPackStatus { get; }

        string KeyId { get; }
    }
}