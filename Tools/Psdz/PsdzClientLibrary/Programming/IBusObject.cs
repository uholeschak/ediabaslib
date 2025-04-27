using PsdzClient.Core;
using PsdzClient.Programming;

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