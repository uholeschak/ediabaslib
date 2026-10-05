using BMW.Rheingold.CoreFramework;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuIdentifier
    {
        string BaseVariant { get; }

        int DiagAddrAsInt { get; }
    }
}
