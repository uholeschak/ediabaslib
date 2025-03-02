using PsdzClient.Core;
using PsdzClient.Programming;

namespace PsdzClient.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IKeypackDetailStatus
    {
        EcuCertCheckingStatus? KeyPackStatus { get; }

        string KeyId { get; }
    }
}