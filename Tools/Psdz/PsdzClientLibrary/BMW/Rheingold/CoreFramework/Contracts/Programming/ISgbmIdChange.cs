using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISgbmIdChange : INotifyPropertyChanged
    {
        string Actual { get; }

        string Target { get; }
    }
}