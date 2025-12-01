using PsdzClient.Core;

namespace PsdzClient.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuIdentifier
    {
        string BaseVariant { get; }

        int DiagAddrAsInt { get; }
    }
}
