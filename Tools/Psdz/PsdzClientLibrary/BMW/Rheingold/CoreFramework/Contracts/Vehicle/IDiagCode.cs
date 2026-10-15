using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IDiagCode : INotifyPropertyChanged
    {
        string DiagnoseCode { get; }

        string DiagnoseCodeSuffix { get; }

        IEnumerable<string> ReparaturPaket { get; }
    }
}