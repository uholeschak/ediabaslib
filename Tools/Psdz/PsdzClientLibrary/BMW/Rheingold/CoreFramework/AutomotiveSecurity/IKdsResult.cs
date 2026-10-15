using BMW.Rheingold.CoreFramework.Contracts;

namespace BMW.Rheingold.CoreFramework.AutomotiveSecurity
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IKdsResult
    {
        int KdsId { get; }

        IBoolResultObject ResultObject { get; }
    }
}
