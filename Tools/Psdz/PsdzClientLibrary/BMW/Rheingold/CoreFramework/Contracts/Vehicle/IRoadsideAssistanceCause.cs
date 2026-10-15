using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IRoadsideAssistanceCause : INotifyPropertyChanged
    {
        uint? Cause { get; }

        uint? TowedAway { get; }

        uint? Repaired { get; }

        uint? Completed { get; }
    }
}
