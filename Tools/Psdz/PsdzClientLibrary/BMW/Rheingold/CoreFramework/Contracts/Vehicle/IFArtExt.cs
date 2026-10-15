using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IFArtExt : INotifyPropertyChanged
    {
        long F_ART_NR { get; }

        string F_ART_TEXT { get; }
    }
}
