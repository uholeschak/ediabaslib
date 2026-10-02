using PsdzClient.Contracts;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IReadPublicKeyResult
    {
        byte[] PublicKey { get; }

        IBoolResultObject Result { get; }
    }
}
