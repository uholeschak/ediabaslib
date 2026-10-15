using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.CoreFramework.Programming.Data.Ecu
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IBusObject
    {
        int Id { get; }

        string Name { get; }

        bool DirectAddress { get; }

        Bus ConvertToBus();
    }
}