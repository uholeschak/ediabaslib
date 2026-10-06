using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuIdentifier
    {
        string BaseVariant { get; }

        int DiagAddrAsInt { get; }
    }
}
