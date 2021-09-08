using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Vehicle
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IDtcContext : INotifyPropertyChanged
    {
        IEnumerable<IDtcUmwelt> F_UW { get; }

        int? F_UW_ANZ { get; }

        long? F_UW_KM { get; }

        double? F_UW_KM_SUPREME { get; }

        long? F_UW_ZEIT { get; }

        double? F_UW_ZEIT_SUPREME { get; }
    }
}
