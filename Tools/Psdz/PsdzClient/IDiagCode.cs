using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IDiagCode : INotifyPropertyChanged
    {
        string DiagnoseCode { get; }

        string DiagnoseCodeSuffix { get; }

        IEnumerable<string> ReparaturPaket { get; }

        bool TeileClearing { get; }
    }
}
