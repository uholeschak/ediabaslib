using System.ComponentModel;

namespace PsdzClient.Core
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuResult : INotifyPropertyChanged
    {
        bool FASTARelevant { get; set; }

        int Format { get; set; }

        uint Length { get; set; }

        bool LengthSpecified { get; set; }

        string Name { get; set; }

        ushort Set { get; set; }

        bool SetSpecified { get; set; }

        object Value { get; set; }
    }
}