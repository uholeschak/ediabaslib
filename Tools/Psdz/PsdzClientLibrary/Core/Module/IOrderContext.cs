using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.ISTA.CoreFramework.SOCAccessor
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface IOrderContext
    {
        IServiceProgram ServiceProgram { get; }

        ISystemContext System { get; }
    }
}
