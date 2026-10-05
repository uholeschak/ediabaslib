using BMW.Rheingold.CoreFramework;
using PsdzClient.Core;

namespace BMW.Rheingold.ISTA.CoreFramework.SOCAccessor
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface ISOCAccessor
    {
        IOrderContext OrderContext { get; }

        ISessionContext SessionContext { get; }
    }
}
