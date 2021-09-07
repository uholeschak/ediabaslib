using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    // [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IDtcUmwelt : INotifyPropertyChanged
    {
        string F_UW_EINH { get; }

        long? F_UW_NR { get; }

        string F_UW_TEXT { get; }

        object F_UW_WERT { get; }
    }
}
