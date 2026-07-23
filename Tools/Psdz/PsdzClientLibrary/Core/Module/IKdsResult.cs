using PsdzClient.Contracts;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.AutomotiveSecurity
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IKdsResult
    {
        int KdsId { get; }

        IBoolResultObject ResultObject { get; }
    }
}
