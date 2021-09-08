using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace PsdzClient.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISvk : INotifyPropertyChanged
    {
        string PROG_DATUM { get; }

        long? PROG_KM { get; }

        int? PROG_TEST { get; }

        IEnumerable<string> XWE_SGBMID { get; }

        ICollection<int> XWE_PROZESSKLASSE_WERT { get; }
    }
}
