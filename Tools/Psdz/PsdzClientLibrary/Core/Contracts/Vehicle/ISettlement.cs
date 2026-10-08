using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISettlement : INotifyPropertyChanged
    {
        string Customer { get; }

        bool? Dealer { get; }

        bool? ServiceInclusive { get; }

        bool? Warranty { get; }
    }
}
