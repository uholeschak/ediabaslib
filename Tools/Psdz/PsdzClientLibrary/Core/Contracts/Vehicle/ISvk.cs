using System.Collections.Generic;
using System.ComponentModel;
using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces.EcuTree;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISvk : INotifyPropertyChanged, IEcuTreeSvk
    {
        string PROG_DATUM { get; }

        long? PROG_KM { get; }

        int? PROG_TEST { get; }
        new IEnumerable<string> XWE_SGBMID { get; }

        ICollection<int> XWE_PROZESSKLASSE_WERT { get; }
    }
}