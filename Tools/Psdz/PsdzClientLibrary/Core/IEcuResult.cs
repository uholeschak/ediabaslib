using System.ComponentModel;

namespace PsdzClient.Core
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuResult : INotifyPropertyChanged
    {
        bool FASTARelevant { get; }

        int Format { get; }

        uint Length { get; }

        bool LengthSpecified { get; }

        string Name { get; }

        ushort Set { get; }

        bool SetSpecified { get; }

        object Value { get; }
    }
}
